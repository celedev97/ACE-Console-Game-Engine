using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace ACE {
    public static class Engine {

        #region Engine settings
        
        private static int windowWidth;
        private static int windowHeight;

        /// <summary>
        /// Set this value to true to lock the Window Size
        /// </summary>
        public static bool lockWindow {
            get { return hiddenLockWindow; }
            set {
                hiddenLockWindow = value;
                if(value) {
                    windowWidth = Console.WindowWidth;
                    windowHeight = Console.WindowHeight;
                }
            }
        }
        private static bool hiddenLockWindow = false;

        private static int frameOffsetX;
        private static int frameOffsetY;

        private static int frameWidth = -1;
        private static int frameHeight = -1;

        /// <summary>
        /// Litterally frame's width... (wow, much immagination)
        /// </summary>
        public static int FRAME_WIDTH { get { return frameWidth; } }

        /// <summary>
        /// Litterally frame's height... (wow, much immagination)
        /// </summary>
        public static int FRAME_HEIGHT { get { return frameHeight; } }

        /// <summary>
        /// The milliseconds that the last game cycle took
        /// This can be used to do dynamical physics calculation.
        /// Thinking about it....
        /// Who the hell would implement physics in a Game Engine that use characters to draw?
        /// </summary>
        public static int deltaTime { get { return hiddenDeltaTime; } }
        private static int hiddenDeltaTime = 0;
        #endregion

        #region Hidden engine constants/variables
        internal static List<GameScript> gameScripts;

        private static List<GameObject> gameObjects;
        private static List<GameObject> newGameObjects;
        private static List<GameObject> toDeleteGameObjects;

        private static readonly object writeLock = new object();
        #endregion

        /// <summary>
        /// This method start the Engine, it's recommended that you set the frame before calling this.
        /// After the Engine start you cannot execute code from the Main anymore, so this is the last instruction you should use in your Main.
        /// </summary>
        /// <param name="fpsCounter">NOT RECOMMENDED: Set to true to show FPS counter, this should be used Only to diagnose heavy FPS drops</param>
        /// <param name="FPSLimit">This is the max FPS that the game will have, set this to -1 to remove the limit. Going higher than 30FPS will result in heavy and ugly flickering</param>
        public static void Start(bool fpsCounter = false, int FPSLimit = -1) {
            #region Initial Setup
            if(fpsCounter) { new Thread(new ThreadStart(printFPS)).Start(); }
            int timeSpan = (timeSpan = 1000/FPSLimit)>0 ? timeSpan : 0;

            Utility.random = new Random();

            //inizializzo lista gameobjects e degli script
            gameObjects = new List<GameObject>();
            newGameObjects = new List<GameObject>();
            toDeleteGameObjects = new List<GameObject>();

            gameScripts = new List<GameScript>();

            //trovo gli script di gioco, li inizializzo, li avvio e me li salvo in una lista
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach(Type type in assembly.GetTypes()) {
                    foreach(Type interfaceType in type.GetInterfaces()) {
                        if(interfaceType == typeof(GameScript) && type != typeof(GameScript) && !type.IsSubclassOf(typeof(GameObject)) && type != typeof(GameObject)) {
                            //questa classe implementa IGameScript, pertanto è uno script di gioco
                            GameScript script = (GameScript)Activator.CreateInstance(type);
                            script.Start();
                            gameScripts.Add(script);
                        }
                    }
                }
            }
            //creo lo stopwatch per il ciclo di gioco
            Stopwatch stopwatch = new Stopwatch();
            #endregion

            #region Game cycle
            //avvio il ciclo di update
            while(true) {
                //reinizializzo lo stopwatch per misurare di questo ciclo
                stopwatch.Restart();

                //lock della finestra
                if(lockWindow) {
                    Console.WindowWidth = windowWidth;
                    Console.WindowHeight = windowHeight;
                }
                //esecuzione degli script generici di gioco
                foreach(GameScript gameScript in gameScripts) {
                    gameScript.Update();
                }

                //esecuzione degli script collegati a un gameObject
                foreach(GameObject gameObject in gameObjects) {
                    //aggiorno i millisecondi dei timer dell'oggetto prima di eseguire l'Update
                    foreach(Timer timer in gameObject.timers) {
                        if(timer.currentMS < timer.msPerTick) timer.currentMS += hiddenDeltaTime;
                        if(timer.ticked()) {
                            if(timer.tickAction != null) timer.tickAction();
                        }
                    }
                    //aggiunta timer appena creati alla lista dei gameObject
                    for(int i = gameObject.newTimers.Count - 1; i >= 0; i--) {
                        gameObject.timers.Add(gameObject.newTimers[i]);
                        gameObject.newTimers.RemoveAt(i);
                    }
                    //cancellazione timer appena cancellati dalla lista dei gameObject
                    for(int i = gameObject.toDeleteTimers.Count - 1; i >= 0; i--) {
                        gameObject.timers.Remove(gameObject.toDeleteTimers[i]);
                        gameObject.toDeleteTimers.RemoveAt(i);
                    }
                    gameObject.Update();
                }

                //aggiunta gameobject appena creati alla lista dei gameObject
                for(int i = newGameObjects.Count - 1; i >= 0; i--) {
                    gameObjects.Add(newGameObjects[i]);
                    newGameObjects.RemoveAt(i);
                }
                //cancellazione gameobject appena cancellati dalla lista dei gameObject
                for(int i = toDeleteGameObjects.Count - 1; i >= 0; i--) {
                    toDeleteGameObjects[i].clean();
                    gameObjects.Remove(toDeleteGameObjects[i]);
                    toDeleteGameObjects.RemoveAt(i);
                }

                //ridisegno i componenti modificati dal ciclo di gioco dei vari script
                Draw();
                //aspetto il numero di MS necessari per arrivare al timestep oppure aspetto 0MS se l'esecuzione dell'update ne ha richiesti più di 100
                if(timeSpan > 0) {
                    int temp = timeSpan - (int)stopwatch.ElapsedMilliseconds;
                    if(temp>0) Thread.Sleep(temp);
                }
                //imposto il deltaTime per le simulazioni fisiche
                hiddenDeltaTime = (int)stopwatch.ElapsedMilliseconds;
            }
            #endregion
        }

        public static void playSound(string filename) {
            var player = new WMPLib.WindowsMediaPlayer();
            /*if(System.Diagnostics) {

            }*/
            player.URL = @"..\..\bin\debug\tribal dance.wav";
        }

        internal static void printFPS() {
            while(true) {
                string print = "FPS: " + (byte)(1000f / hiddenDeltaTime)+"       ";
                Engine.Write(-frameOffsetX, -frameOffsetY, print);
                Thread.Sleep(500);
            }
        }

        #region Public Methods for the frame's settings
        /// <summary>
        /// Draw the Frame, it's recommended to call this function just once and only before starting the Engine.
        /// </summary>
        public static void DrawFrame() {
            //disegno linea verticale
            for(int i = frameOffsetX - 1; i <= Console.WindowWidth - (frameOffsetX - 1); i++) {
                Console.SetCursorPosition(i, frameOffsetY - 1);
                Console.Write("═");
            }
            //disegno linea verticale bassa
            for(int i = frameOffsetX - 1; i <= Console.WindowWidth - (frameOffsetX - 1); i++) {
                Console.SetCursorPosition(i, Console.WindowHeight - (frameOffsetY - 1));
                Console.Write("═");
            }
            //disegno linea orizzontale
            for(int i = frameOffsetY - 1; i <= Console.WindowHeight - (frameOffsetY - 1); i++) {
                Console.SetCursorPosition(frameOffsetX - 1, i);
                Console.Write("║");
            }
            //disegno linea orizzontale destra
            for(int i = frameOffsetY - 1; i <= Console.WindowHeight - (frameOffsetY - 1); i++) {
                Console.SetCursorPosition(Console.WindowWidth - (frameOffsetX - 1), i);
                Console.Write("║");
            }

            //disegno gli angoli
            Console.SetCursorPosition(frameOffsetX - 1, frameOffsetY - 1);
            Console.Write("╔");
            Console.SetCursorPosition(frameOffsetX - 1, Console.WindowHeight - (frameOffsetY - 1));
            Console.Write("╚");
            Console.SetCursorPosition(Console.WindowWidth - (frameOffsetX - 1), frameOffsetY - 1);
            Console.Write("╗");
            Console.SetCursorPosition(Console.WindowWidth - (frameOffsetX - 1), Console.WindowHeight - (frameOffsetY - 1));
            Console.Write("╝");
        }

        /// <summary>
        /// Set the game frame offset from the border of the console. It's recommended to use this function just once and only before drawing the frame
        /// </summary>
        /// <param name="frameOffsetX">The offset of the frame from the border of the console on the X axis</param>
        /// <param name="frameOffsetY">The offset of the frame from the border of the console on the Y axis</param>
        public static void SetFrameDimension(int frameOffsetX, int frameOffsetY) {
            Engine.frameOffsetX = frameOffsetX;
            Engine.frameOffsetY = frameOffsetY;
            if(frameWidth > -1 || frameHeight > -1) {
                throw new Exception("You can't set the frame dimensions twice");
            } else {
                //TODO: in caso di problemi moltiplica per 2
                frameWidth = Console.WindowWidth - (frameOffsetX * 2);
                frameHeight = Console.WindowHeight - (frameOffsetY * 2);
            }
        }
        #endregion

        #region Hidden engine methods for drawing
        internal static void Draw() {
            sortGameObjectByZ();
            foreach(GameObject gameObject in gameObjects) {
                gameObject.clean();
                gameObject.draw();
            }
        }

        private static void sortGameObjectByZ() {
            bool nothingReplaced;
            do {
                nothingReplaced = true;
                for(int i = gameObjects.Count - 1; i > 0 && nothingReplaced; i--) {
                    if(gameObjects[i].drawLayer < gameObjects[i - 1].drawLayer) {
                        GameObject temp = gameObjects[i];
                        gameObjects[i] = gameObjects[i - 1];
                        gameObjects[i - 1] = temp;
                        nothingReplaced = false;
                    }
                }
            } while(!nothingReplaced);
        }

        internal static void SetCursorPosition(int x, int y) {
            Console.SetCursorPosition(x+frameOffsetX,y+frameOffsetY);
        }

        internal static void Write(char content) {
            Console.Write(content);
        }
        internal static void Write(string content) {
            Console.Write(content);
        }
        internal static void Write(int x, int y, char content) {
            lock(writeLock) {
                SetCursorPosition(x, y);
                Write(content);
            }
        }
        internal static void Write(int x, int y, string content) {
            lock(writeLock) {
                SetCursorPosition(x, y);
                Write(content);
            }
        }
        #endregion

        #region Methods for GameObject operations
        #region Methods for getting informations
        /// <summary>
        /// Get all the gameObject currently in the Game
        /// </summary>
        /// <returns>An array that contains all the gameObject that are not deleted</returns>
        public static GameObject[] findGameObjects() {
            GameObject[] output = new GameObject[newGameObjects.Count+gameObjects.Count];
            for(int i = 0; i < gameObjects.Count; i++) {
                output[i] = gameObjects[i];
            }
            for(int i = gameObjects.Count; i < (gameObjects.Count + newGameObjects.Count); i++) {
                output[i] = newGameObjects[i - gameObjects.Count];
            }
            return gameObjects.ToArray();
        }
        /// <summary>
        /// Get all the gameObjects of a given GameObject subClass
        /// </summary>
        /// <typeparam name="GameObjectClass">The type of the gameObjects to find</typeparam>
        public static List<GameObjectClass> findGameObjects<GameObjectClass>() {
            List <GameObjectClass> output = new List<GameObjectClass>();
            foreach(GameObject gameObject in gameObjects) {
                if(typeof(GameObjectClass).IsInstanceOfType(gameObject)) {
                    output.Add((GameObjectClass)Convert.ChangeType(gameObject, typeof(GameObjectClass)));
                }
            }
            foreach(GameObject gameObject in newGameObjects) {
                if(typeof(GameObjectClass).IsInstanceOfType(gameObject)) {
                    output.Add((GameObjectClass)Convert.ChangeType(gameObject, typeof(GameObjectClass)));
                }
            }
            return output;
        }

        /// <summary>
        /// Check if a GameObject exists currently
        /// </summary>
        /// <param name="gameObject">The GameObject to search</param>
        public static bool gameObjectExists(GameObject gameObject) {
            return gameObjects.Contains(gameObject) || newGameObjects.Contains(gameObject);
        }
        /// <summary>
        /// Check if a GameObject is destroyed
        /// </summary>
        /// <param name="gameObject">The GameObject to search</param>
        /// <returns>true if the gameObject doesn't exists</returns>
        public static bool isGameObjectDestroyed(GameObject gameObject) {
            return ((!gameObjects.Contains(gameObject)) && !newGameObjects.Contains(gameObject)) || toDeleteGameObjects.Contains(gameObject);
        }
        #endregion

        #region Methods for modifying the GameObject list
        /// <summary>
        /// NOT RECOMMENDED: This method add forcefully a gameObject in the gameObjects list, when a gameObject is created it is automaticaly added to the list, don't use this method if you are not sure what you are doing
        /// </summary>
        /// <param name="gameObject">The GameObject to add</param>
        public static void AddGameObject(GameObject gameObject) {
            newGameObjects.Add(gameObject);
        }

        /// <summary>
        /// Delete a gameObject from the gameObject list
        /// </summary>
        /// <param name="gameObject">The gameObject to remove</param>
        public static void DeleteGameObject(GameObject gameObject) {
            toDeleteGameObjects.Add(gameObject);
        }

        #endregion
        #endregion
        /// <summary>
        /// This is a static class that contains some method that can be used to do various things
        /// </summary>
        public static class Utility {
            internal static Random random;

            /// <summary>
            /// This method generate a random int number
            /// </summary>
            /// <param name="max">The maximum number generated</param>
            /// <returns>A number from 0 to max (both included in the range)</returns>
            public static int RandomInt(int max) {
                return (int)(random.NextDouble() * (max + 1));
            }

            /// <summary>
            /// Wait... you really need me to explain this... ?
            /// A simple function that count the occurrence of a string in another string
            /// </summary>
            /// <param name="haystack">The large string</param>
            /// <param name="needle">The small string that must be searched in the larger one</param>
            /// <returns></returns>
            public static int countStringOccurrences(string haystack, string needle) {
                int count = 0;
                int nextIndex = 0;
                while((nextIndex = haystack.IndexOf(needle,nextIndex))>-1) {
                    count++;
                }
                return count;
            }

            internal static string[] StringToDesignArray(string designString) {
                return designString.Replace("\r\n", "\n").Split('\n'); ;
            }
        }
    }
}

