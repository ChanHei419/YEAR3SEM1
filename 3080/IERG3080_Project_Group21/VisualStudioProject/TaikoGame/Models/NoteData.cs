namespace TaikoGame
{
    public class NoteData
    {
        public double Time { get; set; }
        public bool IsRed { get; set; }

        public NoteData(double time, bool isRed)
        {
            Time = time;
            IsRed = isRed;
        }
    }
}
