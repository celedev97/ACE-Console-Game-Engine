using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACE {
    public class Timer {
        #region Variables and properties
        private int hiddenID;
        public int ID { get => hiddenID; }

        private int hiddenMSPerTick;
        public int msPerTick {
            get => hiddenMSPerTick;
            set {
                if(value > 0) {
                    hiddenMSPerTick = value;
                } else {
                    throw new Exception("Specified MS for Timer are less than 1");
                }
            }
        }

        internal int currentMS;

        public Action tickAction;

        public bool restartFlag;
        #endregion

        #region Constructors
        public Timer(int timerID, int msPerTick, Action tickAction, bool restartFlag = true, int currentMS = 0) {
            this.restartFlag = restartFlag;
            this.tickAction = tickAction;
            hiddenID = timerID;
            set(msPerTick, currentMS);
        }
        #endregion

        #region Methods
        public bool ticked() {
            bool output;
            if((output = msPerTick <= currentMS) && restartFlag) {
                restart();
            }
            return output;
        }

        public void restart() {
            currentMS = 0;
        }

        public void set(int msPerTick, int currentMS = 0) {
            this.msPerTick = msPerTick;
            this.currentMS = currentMS;
        }

        #endregion
    }
}
