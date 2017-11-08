using System;
using System.Collections.Generic;

namespace ACE {
    /// <summary>
    /// This class represent a Game Object, a Game Object is an Object wich is drawn on the screen,
    /// it can have collision, it can move, it can change appeareance and so on. 
    /// </summary>
    public class GameObject:GameScript {
        #region public variables
        /// <summary>
        /// The position of the GameObject on the X axis
        /// </summary>
        public int xPosition;

        /// <summary>
        /// The position of the GameObject on the X axis
        /// </summary>
        public int yPosition;

        /// <summary>
        ///  This is the foregroundColor used to print this gameObject
        /// </summary>
        public ConsoleColor foreColor;

        /// <summary>
        ///  This is the backgroundColor used to print this gameObject
        /// </summary>
        public ConsoleColor backColor;


        /// <summary>
        /// The position of the GameObject on the Z axis
        /// (note: the Z axis is Only used for drawing's priority)
        /// </summary>
        public int drawLayer;

        /// <summary>
        /// ...C'mon, don't let me do this, you know what this variable is about
        /// </summary>
        public bool visible;

        /// <summary>
        /// If set to true the Object won't go into other Physical Object, if set to false this object will be able to pass through all the other objects
        /// </summary>
        public bool physicalObject;

        /// <summary>
        /// Returns the object that collided with this Object in the last movement, can be NULL
        /// </summary>
        public GameObject lastCollision { get { return hiddenLastCollision; } }

        /// <summary>
        /// NOT RECOMMENDED: the list of the timers used by this GameObject, you should use the default functions related to timers to create and delete them, but...
        /// y'know, i don't really like closed codes in wich you can't fuck up everything, dont'cha agree?
        /// </summary>
        protected internal List<Timer> timers;
        protected internal List<Timer> newTimers;
        protected internal List<Timer> toDeleteTimers;

        public int height { get => hiddenHeight; }
        public int width { get => hiddenWidth; }
        #endregion

        #region protected variables
        /// <summary>
        /// NOT RECOMMENDED: this is the matrix that represent the aspect of the GameObject, you shouldn't change it directly unless you have to do something really particular
        /// </summary>
        protected char[,] repMatrix;

        /// <summary>
        ///  NOT RECOMMENDED: this is old position on the X axis of the GameObject before the last movement, you shouldn't touch it unless you want to override the move function
        /// </summary>
        protected int oldX;

        /// <summary>
        ///  NOT RECOMMENDED: this is old position on the Y axis of the GameObject before the last movement, you shouldn't touch it unless you want to override the move function
        /// </summary>
        protected int oldY;

        /// <summary>
        ///  NOT RECOMMENDED: this is the calculated width of the GameObject, don't change it unless you know what you are doing
        /// </summary>
        protected int hiddenWidth;

        /// <summary>
        ///  NOT RECOMMENDED: this is the calculated height of the GameObject, don't change it unless you know what you are doing
        /// </summary>
        protected int hiddenHeight;
        
        /// <summary>
        /// This contain the GameObject that collided with this one in the last movement (NULL if there was no collision)
        /// </summary>
        protected GameObject hiddenLastCollision;
        #endregion

        #region Constructors
        #region Constructors that require the aspect of the gameObject
        /// <summary>
        /// Create a GameObject
        /// </summary>
        /// <param name="design">The string that represent the aspect of the GameObject</param>
        /// <param name="x">The position of the GameObject on the X axis</param>
        /// <param name="y">The position of the GameObject on the Y axis</param>
        /// <param name="drawLayer">The priority for drawing this Object</param>
        /// <param name="physicalObject">A bool that indicate if the Object is Physical or not</param>
        public GameObject(string design, int x, int y, int drawLayer, bool physicalObject = true) {
            Initialize(x, y);
            setDesign(design);
            this.drawLayer = drawLayer;
            this.physicalObject = physicalObject;
        }

        /// <summary>
        /// Create a GameObject
        /// </summary>
        /// <param name="design">A char matrix that represent the aspect of the Object</param>
        /// <param name="x">The position of the GameObject on the X axis</param>
        /// <param name="y">The position of the GameObject on the Y axis</param>
        /// <param name="drawLayer">The priority for drawing this Object</param>
        /// <param name="physicalObject">A bool that indicate if the Object is Physical or not</param>
        public GameObject(char[,] design, int x, int y, int drawLayer, bool physicalObject = true) {
            Initialize(x, y);
            setDesign(design);
            this.drawLayer = drawLayer;
            this.physicalObject = physicalObject;
        }

        /// <summary>
        /// Create a GameObject
        /// </summary>
        /// <param name="design">A char matrix that represent the aspect of the Object</param>
        /// <param name="x">The position of the GameObject on the X axis</param>
        /// <param name="y">The position of the GameObject on the Y axis</param>
        /// <param name="physicalObject">A bool that indicate if the Object is Physical or not</param>
        public GameObject(char[,] design, int x, int y, bool physicalObject = true) : this(design, x, y, 0, physicalObject) { }

        /// <summary>
        /// Create a GameObject
        /// </summary>
        /// <param name="design">The string that represent the aspect of the GameObject</param>
        /// <param name="x">The position of the GameObject on the X axis</param>
        /// <param name="y">The position of the GameObject on the Y axis</param>
        /// <param name="physicalObject">A bool that indicate if the Object is Physical or not</param>
        public GameObject(string design, int x, int y, bool physicalObject = true) : this(design, x, y, 0, physicalObject) { }
        #endregion

        #region Constructors that don't require the aspect of the gameObject

        /// <summary>
        /// Create a GameObject
        /// </summary>
        /// <param name="x">The position of the GameObject on the X axis</param>
        /// <param name="y">The position of the GameObject on the Y axis</param>
        /// <param name="drawLayer">The priority for drawing this Object</param>
        /// <param name="physicalObject">A bool that indicate if the Object is Physical or not</param>
        public GameObject(int x, int y, int drawLayer, bool physicalObject = true) {
            Initialize(x, y);
            this.drawLayer = drawLayer;
            this.physicalObject = physicalObject;
        }

        /// <summary>
        /// Create a GameObject
        /// </summary>
        /// <param name="x">The position of the GameObject on the X axis</param>
        /// <param name="y">The position of the GameObject on the Y axis</param>
        /// <param name="physicalObject">A bool that indicate if the Object is Physical or not</param>
        public GameObject(int x, int y, bool physicalObject = true) : this(x, y, 0, physicalObject) { }
        #endregion

        private void Initialize(int x, int y) {
            timers = new List<Timer>();
            newTimers = new List<Timer>();
            toDeleteTimers = new List<Timer>();

            //imposto le impostazioni di default, verranno poi cambiate dai costruttori se specificate
            visible = true;
            physicalObject = true;

            //aggiungo il gameObject appena creato alla lista dei gameObject
            Engine.AddGameObject(this);

            //imposto la sua posizione
            xPosition = x;
            yPosition = y;

            //imposto il colore di default
            foreColor = ConsoleColor.White;
            backColor = ConsoleColor.Black;

            hiddenLastCollision = null;

            //avvio il metodo di inizializzazione del gameObject
            Start();
        }
        #endregion

        #region Method for drawing the GameObject
        internal void clean() {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            for(int y = 0; y < repMatrix.GetLength(0); y++) {
                for(int x = 0; x < repMatrix.GetLength(1); x++) {
                    Engine.Write(oldX + x, oldY + y, ' ');
                }
            }
        }

        internal void draw() {
            Console.ForegroundColor = foreColor;
            Console.BackgroundColor = backColor;
            for(int y = 0; y < repMatrix.GetLength(0); y++) {
                for(int x = 0; x < repMatrix.GetLength(1); x++) {
                    Engine.Write(xPosition + x, yPosition + y, repMatrix[y, x]);
                }
            }
            oldX = xPosition;
            oldY = yPosition;
        }

        #endregion

        #region Method to use Engine timers
        //struttura del timer = {ID, RATEO, CURRENT_MS}

        /// <summary>
        /// This method create a timer that you can check in the Update method
        /// </summary>
        /// <param name="timerID">the ID of the timer, different gameObject can have the same ID for different timer</param>
        /// <param name="msPerTick">The number of milliseconds necessary for this timer to tick</param>
        /// <param name="currentMS">The current number of milliseconds passed from the last tick</param>
        public Timer createTimer(int timerID, int msPerTick, Action tickAction, int currentMS) {
            return createTimer(timerID, msPerTick, tickAction, true, currentMS);
        }
        public Timer createTimer(int timerID, int msPerTick, Action tickAction, bool restartFlag = true, int currentMS = 0) {
            if(msPerTick < 1) throw new Exception("You cannot create a timer that tick every less than one second");
            //cerco il timer, se esiste cambio il tuo rateo di tick
            bool timerExists = false;
            foreach(Timer timer in timers) {
                if(timer.ID == timerID) {
                    timerExists = true;
                }
            }
            //se non esiste ne creo uno nuovo
            if(timerExists) {
                throw new Exception("A timer with the ID:" + timerID + " already exists");
            } else {
                Timer timer = new Timer(timerID, msPerTick, tickAction, restartFlag, currentMS);
                newTimers.Add(timer);
                return timer;
            }
        }

        public Timer getTimer(int timerID) {
            foreach(Timer timer in timers) {
                if(timer.ID == timerID) {
                    return timer;
                }
            }
            return null;
        }

        public bool removeTimer(int timerID) {
            int index = -1;
            for(int i = 0; i < timers.Count && index == -1; i++) {
                if(timers[i].ID == timerID) {
                    index = i;
                    toDeleteTimers.Add(timers[i]);
                }
            }
            if(index != -1) {
                return true;
            }
            return false; ;
        }

        #endregion

        #region Methods related to GameObject's appearance
        #region Methods to alter GameObject's appearance

        /// <summary>
        /// This replace the design with a new one of the same size
        /// </summary>
        /// <param name="design">The new design as a char matrix</param>
        /// <returns>true if the new design is compatible with the last one</returns>
        public bool replaceDesign(char[,] design) {
            if(hiddenHeight != design.GetLength(0) || hiddenWidth != design.GetLength(1)) return false;
            repMatrix = design;
            return true;
        }

        /// <summary>
        /// This replace the design with a new one of the same size
        /// </summary>
        /// <param name="design">The new design as a string</param>
        /// <returns>true if the new design is compatible with the last one</returns>
        public bool replaceDesign(string design) {
            int[] designDimensions = getDesignDimensions(Engine.Utility.StringToDesignArray(design));
            if(hiddenHeight != designDimensions[0] || hiddenWidth != designDimensions[1]) return false;

            setDesign(design);
            return true;
        }
        #endregion

        #region Methods to set or reset GameObject's appearance
        /// <summary>
        /// NOT RECOMMENDED: this method set a new design without checking if the size of the new design is the same.
        /// It does set the new width and height, but using this method can still lead to unexpected behaviours
        /// </summary>
        /// <param name="design">The new design as a string</param>
        protected void setDesign(string design) {
            //divido la stringa per capire quanto dev'essere grande la matrice per disegnare l'oggetto;
            string[] designArray = Engine.Utility.StringToDesignArray(design);
            int[] designDimensions = getDesignDimensions(designArray);

            //imposto le dimensioni del gameObject
            hiddenHeight = designDimensions[0];
            hiddenWidth = designDimensions[1];

            //compongo la matrice riempendola con i caratteri della stringa
            repMatrix = new char[hiddenHeight, hiddenWidth];
            for(int y = 0; y < designArray.Length; y++) {
                for(int x = 0; x < designArray[y].Length; x++) {
                    repMatrix[y, x] = designArray[y][x];
                }
            }
        }

        /// <summary>
        /// NOT RECOMMENDED: this method set a new design without checking if the size of the new design is the same.
        /// It does set the new width and height, but using this method can still lead to unexpected behaviours
        /// </summary>
        /// <param name="design">The new design as a char matrix</param>
        protected void setDesign(char[,] design) {
            repMatrix = design;
            hiddenHeight = repMatrix.GetLength(0);
            hiddenWidth = repMatrix.GetLength(1);
        }

        #endregion

        #region Methods to get informations about the Game
        /// <summary>
        /// Get the dimensions of a string design
        /// </summary>
        /// <param name="design">The design that should be checked</param>
        /// <returns></returns>
        protected int[] getDesignDimensions(string[] design) {
            int height = design.Length;
            int width = design[0].Length;
            foreach(string line in design) {
                width = line.Length > width ? line.Length : width;
            }
            return new int[] { height, width };
        }
        #endregion
        #endregion

        #region Methods related to the movement of the GameObject
        /// <summary>
        /// This function moves the gameObject of a certain offset
        /// </summary>
        /// <param name="xOffset">The offset of the movement of the GameObject on the X axis</param>
        /// <param name="yOffset">The offset of the movement of the GameObject on the Y axis</param>
        /// <returns>This return true if the gameObject moved without collisions, false otherwise</returns>
        public virtual bool move(int xOffset, int yOffset) {
            //muovo il GameObject controllando che non esca dal Frame
            xPosition += xOffset;
            yPosition += yOffset;

            if(xPosition > Engine.FRAME_WIDTH +1 -hiddenWidth || xPosition < 0 || yPosition > Engine.FRAME_HEIGHT +1 - hiddenHeight || yPosition < 0) {
                xPosition -= xOffset;
                yPosition -= yOffset;
                hiddenLastCollision = null;
                return false;
            }
            
            //controllo se ha sbattuto contro qualcosa
            if((hiddenLastCollision = checkCollisions()) != null) {
                if(physicalObject) {
                    xPosition -= xOffset;
                    yPosition -= yOffset;
                }
                OnCollisionEnter();
                hiddenLastCollision.hiddenLastCollision = this;
                hiddenLastCollision.OnCollisionEnter();
                return false;
            }
            return true;
        }
        #endregion

        #region Methods related to collisions

        /// <summary>
        /// Check if this object collides with some other objects.
        /// </summary>
        /// <returns>This method return ONLY the first gameObject found that collides, NOT ALL OF THEM.</returns>
        protected GameObject checkCollisions() {
            foreach(GameObject gameObject2 in Engine.findGameObjects()) {
                if(this != gameObject2) {
                    if(this.collide(gameObject2)) {
                        return gameObject2;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Check if this GameObject is colliding with another one, WITHOUT firing OnCollisionEnter()
        /// </summary>
        /// <param name="gameObject2"></param>
        /// <returns></returns>
        public bool collide(GameObject gameObject2) {
            //distanze calcolate dai centri
            float distX = Math.Abs((xPosition+ hiddenWidth/2f) - (gameObject2.xPosition + gameObject2.hiddenWidth/2f));
            float distY = Math.Abs((yPosition + hiddenHeight/2f) - (gameObject2.yPosition + gameObject2.hiddenHeight/2f));
            //metà della somma delle dimensioni
            float widthSum = (hiddenWidth + gameObject2.hiddenWidth)/2f;
            float heightSum = (hiddenHeight + gameObject2.hiddenHeight)/2f;

            if(distX >= widthSum || distY >= heightSum) {
                return false;
            }
            
            return true;
        }

        #endregion

        #region Virtual Methods that should be overridden by the Game Programmer in his scripts
        /// <summary>
        /// This method is called every game cycle, here you should check your timers, input flags, process movements and so on.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// This method is called once after the Object is created and before the first Update.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// This method is called when this object is colliding with another Object, the collision data are in the lastCollision property
        /// </summary>
        public virtual void OnCollisionEnter() { }
        #endregion
    }
}
