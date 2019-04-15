using System;

namespace sIRC
{
    class Spinner
    {
        private const int speed = 20;

        private int lastFrame;
        private int count;

        public Spinner()
        {
            lastFrame = -1;
        }

        public virtual void Reset()
        {
            lastFrame = -1;
            count = 0;           
        }

        public virtual void Turn()
        {
            int frame = (DateTime.Now.Second * speed + DateTime.Now.Millisecond / (1000 / speed)) % 4;

            if (lastFrame == frame)
                return;

            Log.ClearLastCharacter();

            if (count < Console.BufferWidth / 2)
            {
                Console.Write(@"-");
                count++;
            }
                
            switch (frame % 4)
            {
                case 0: Console.Write(@"|"); break;
                case 1: Console.Write(@"/"); break;
                case 2: Console.Write(@"-"); break;
                case 3: Console.Write(@"\"); break;
            }

            lastFrame = frame;
        }
    }
}
