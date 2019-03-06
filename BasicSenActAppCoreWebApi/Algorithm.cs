using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dotnet_etcd;
using Etcdserverpb;
using Google.Protobuf;

namespace BasicSenActAppCoreWebApi
{
    public class Algorithm
    {
        private StateMachine state;
        private OperationLog log;

        public bool IsLeader { get; set; }
        public int ReqNumber { get; private set; }
        private int generatedReqNum;

        private int maxNumOfReplicas;
        private int ackNumOfReplicas;

        private Dictionary<int, bool> acceptedReqs;
        private Dictionary<int, bool> readyToBeExecuted;

        EtcdClient client;
        public bool ReqExists { get; set; }
        string myId;
        Timer t;
        bool generateResponse;

        public Algorithm(int mnor)
        {
            state = new StateMachine();
            log = new OperationLog();
            acceptedReqs = new Dictionary<int, bool>();
            readyToBeExecuted = new Dictionary<int, bool>();

            maxNumOfReplicas = mnor;
            client = new EtcdClient("10.108.237.143", 4000);
            myId = (new Guid()).ToString();
        }

        private void TimerTick(object state)
        {
            if (!readyToBeExecuted[ReqNumber])
                StartVoting();
            t = null;
        }

        private void StartTimer()
        {
            t = new Timer(TimerTick, new object(), 2500, Timeout.Infinite);
        }

        public int GetState()
        {
            return state.I;
        }

        private int SetState()
        {
            state.IncI();
            return GetState();
        }

        public void ExecuteRequest()
        {
            StartTimer();
            GenerateReqNumber();
            RecommendReqNumber(generatedReqNum, "IncState"); //salje zahtev
        }

        public void CreateRequest()
        {
            string s = client.GetVal("req");
            if (s == null)
            {
                //ogranicenje, redom okidaj inicijalizaciju na cvorovima da ne dobijes 2 lidera
                client.Put("req", "");
                client.Put("leader", myId);
                IsLeader = true;
            }
            else
                IsLeader = false;
            ReqExists = true;
            client.Watch("leader", EtcdElected);
        }

        public void WatchRequest()
        {
            client.Watch("req", EtcdNotification); //request se postavlja
        }

        public int GenerateResponse()
        {
            generateResponse = true; //private promenljiva da je replika ta koja treba da generise odgovor, odnosno ona je primila zahtev
            while (!log.Log[0].ResponseReady)
                Thread.Sleep(50);
            log.Log.RemoveAt(0);
            generateResponse = false;
            return state.I;
        }

        //asinhroni prijem iz etcd-a
        public void EtcdNotification(WatchResponse response) //proveri na watch da li se dobija value ili key
        {
            string value = response.Events[0].Kv.Value.ToString();
            string[] s = value.Split('|');
            Notification(Int32.Parse(s[0]), s[1]);
        }

        private void Notification(int reqNum, string method)
        {
            //kada se primi zahtev
            if (method == "") //inicijalno
            {
                if(string.IsNullOrEmpty(client.GetVal("leader")))
                    StartVoting();
                return; //neko je inicijalizovao req 
            }
            if (t == null)
                StartTimer();
            if (reqNum == 0)
            {
                if (!IsLeader)
                    return;
            
                Generate();
                RecommendReqNumber(generatedReqNum, "IncState");
            }
            else if (!IsLeader)
            {
                //ackNumOfReplicas=1; //od lidera je stigao zahtev sto znaci da je on sam potvrdio vec jednom (to je tacno ali vadim sve potvrde)
                if (!acceptedReqs.ContainsKey(reqNum))
                {
                    AcceptNotification(reqNum, method);
                }
                CheckToAddAndExecuteOp(reqNum, method);
            }
            else
            {
                CheckToAddAndExecuteOp(reqNum, method);
            }
        }

        private void AcceptNotification(int reqNum, string method)
        {
            client.Put("ack" + reqNum.ToString() + "|" + Guid.NewGuid().ToString(), "");
            acceptedReqs[reqNum] = true;
        }

        private void CheckToAddAndExecuteOp(int reqNum, string method)
        {
            Operation op = new Operation { Name = method, Number = reqNum };
            if (!log.Log.Contains(op))
            {
                log.Log.Add(op);
            }
            CheckToExecuteOp(op);
        }

        private void CheckToExecuteOp(Operation op)
        {
            ackNumOfReplicas = client.GetRange("ack" + op.Number.ToString() + "|").Kvs.Count;
            if (ackNumOfReplicas > ((maxNumOfReplicas - 1) / 2))
            {
                //stop timer
                ReqNumber = op.Number;
                readyToBeExecuted[op.Number] = true;
                t.Dispose();

                if (IsLeader)
                {
                    //put req in history so that any other lider will know what to do when elected
                    client.Put("req|" + op.Number, op.Name);
                }

                if (log.Log[0].Equals(op))
                {
                    ExecuteOp();
                }
            }
        }

        private void ExecuteOp()
        {
            int i = SetState();
            if (generateResponse)
                log.Log[0].ResponseReady = true;
            else
                log.Log.RemoveAt(0);
        }

        private void StartVoting()
        {
            client.Put("leso|" + myId, "");
            if (client.GetRange("leso|").Kvs[0].Value.ToString().Equals(myId))
            {
                client.Put("leader", myId);
            };
        }

        //asinhroni prijem iz etcd-a
        public void EtcdElected(WatchResponse response) //proveri na watch da li se dobija value ili key
        {
            string value = response.Events[0].Kv.Value.ToString();
            IsLeader = value.Equals(myId);
            if(IsLeader)
            {
                ExecuteHistoryRequests();
            }
        }

        private void ExecuteHistoryRequests()
        {
            var reqs = client.GetRange("req|").Kvs;

            foreach (var r in reqs)
            {
                if (Int32.Parse(r.Key.ToString().Split('|')[1]) <= ReqNumber)
                    continue;
                if (r.Value.ToString().Equals("IncState")) //moglo i za druge metode...
                    SetState();
            }
        }

        private void RecommendReqNumber(int reqNum, string method)
        {
            //posalji zahtev etcd-u 
            string req = reqNum.ToString();
            client.Put("req", req +"|"+ method);
            if (IsLeader)
            {
                acceptedReqs[reqNum] = true;
                client.Put("ack" + req + "|", Guid.NewGuid().ToString());
            }
        }

        private void GenerateReqNumber()
        {
            if (IsLeader)
                Generate();
            else
                generatedReqNum = 0;
        }

        private void Generate()
        {
            generatedReqNum = ++ReqNumber;
        }

    }
}
