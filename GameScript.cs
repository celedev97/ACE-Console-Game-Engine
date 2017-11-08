namespace ACE {
    public interface GameScript {
        /// <summary>
        /// A method that keep being called continuosly
        /// </summary>
        void Update();

        /// <summary>
        /// This method is called just once, just after the script initialization
        /// </summary>
        void Start();
    }
}