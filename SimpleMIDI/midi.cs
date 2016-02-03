using System.IO;
using System.Collections.Generic;
using Priority_Queue;

namespace SimpleMIDI
{

    public class Note : FastPriorityQueueNode
    {
        static public readonly byte DEFAULT_VELOCITY = 40;

        private long start;
        private int duration;
        private byte key;
        private byte velocity;
		
		public Note(long start, int duration, byte key) {
			this.start = start;
			this.duration = duration;
			this.key = key;
			velocity = DEFAULT_VELOCITY;
		}

        public Note(long start, int duration, byte key, byte velocity)
        {
            this.start = start;
            this.duration = duration;
            this.key = key;
            this.velocity = velocity;
        }

        internal int GetDuration()
        {
            return duration;
        }

        internal byte[] MidiStart(long deltaStart)
        {
            long delta = start - deltaStart;
            int vl = 0;
            for (long i = delta; i >= 128; i = i / 128) vl++;
            byte[] toReturn = new byte[vl + 4];
            toReturn[vl] = (byte) (delta % 128);
            for (int i = vl-1; delta >= 128; i--)
            {
                delta = delta / 128;
                toReturn[i] = (byte) (128 + (delta%128));
            }
            toReturn[vl + 1] = 144;
            toReturn[vl + 2] = key;
            toReturn[vl + 3] = velocity;
            return toReturn;
        }

        internal byte[] MidiEnd(long deltaStart)
        {
            long delta = start + duration - deltaStart;
            int vl = 0;
            for (long i = delta; i >= 128; i = i / 128) vl++;
            byte[] toReturn = new byte[vl + 4];
            toReturn[vl] = (byte)(delta % 128);
            for (int i = vl - 1; delta >= 128; i--)
            {
                delta = delta / 128;
                toReturn[i] = (byte)(128 + (delta % 128));
            }
            toReturn[vl + 1] = 144;
            toReturn[vl + 2] = key;
            toReturn[vl + 3] = 0;
            return toReturn;
        }

    }

    public class MIDIGenerator
    {
        static public readonly ushort DEFAULT_TICKRATE = 128;

        HashSet<Note> notes;
        int numNotes;
        ushort tickRate;
        string trackName;

        public MIDIGenerator(string trackName)
        {
            notes = new HashSet<Note>();
            numNotes = 0;
            tickRate = DEFAULT_TICKRATE;
            this.trackName = trackName;
        }

        public MIDIGenerator(string trackName, ushort tickRate)
        {
            notes = new HashSet<Note>();
            numNotes = 0;
            this.tickRate = tickRate;
            this.trackName = trackName;
        }

        public bool AddNote(Note note)
        {
            if (notes.Add(note))
            {
                numNotes++;
                return true;
            }
            return false;
        }

        public bool RemoveNote(Note note)
        {
            if (notes.Remove(note))
            {
                numNotes--;
                return true;
            }
            return false;
        }

        public string GetTrackName()
        {
            return trackName;
        }

        public void ChangeTrackName(string trackName)
        {
            this.trackName = trackName;
        }

        public ushort GetTickRate()
        {
            return tickRate;
        }

        public void ChangeTickRate(ushort tickRate)
        {
            this.tickRate = tickRate;
        }

        public void WriteMIDI(string path)
        {
            HashSet<Note>.Enumerator enumerator = notes.GetEnumerator();
            FastPriorityQueue<Note> startQueue = new FastPriorityQueue<Note>(numNotes);
            FastPriorityQueue<Note> endQueue = new FastPriorityQueue<Note>(numNotes);
            while (enumerator.MoveNext())
            {
                startQueue.Enqueue(enumerator.Current, enumerator.Current.Priority);
            }
            using (BinaryWriter writer = new BinaryWriter(File.Open(path+trackName + ".mid", FileMode.Create)))
            {
                writer.Write(OneTrackHeader());
                writer.Write(StandardTrackHeader());
                long deltaStart = 0;
                do
                {
                    if (endQueue.Peek() == null)
                    {
                        Note note = startQueue.Dequeue();
                        writer.Write(note.MidiStart(deltaStart));
                        deltaStart = (long) note.Priority;
                        endQueue.Enqueue(note, note.Priority+note.GetDuration());
                    } else if (startQueue.Peek() == null)
                    {
                        Note note = endQueue.Dequeue();
                        writer.Write(note.MidiEnd(deltaStart));
                        deltaStart = (long) note.Priority;
                    } else
                    {
                        if (endQueue.Peek().Priority > startQueue.Peek().Priority)
                        {
                            Note note = startQueue.Dequeue();
                            writer.Write(note.MidiStart(deltaStart));
                            deltaStart = (long)note.Priority;
                            endQueue.Enqueue(note, note.Priority+note.GetDuration());
                        } else
                        {
                            Note note = endQueue.Dequeue();
                            writer.Write(note.MidiEnd(deltaStart));
                            deltaStart = (long)note.Priority;
                        }
                    }
                } while (startQueue.Count + endQueue.Count > 0);
                writer.Write(TrackEnd());
            }
        }

        private byte[] OneTrackHeader()
        {
            byte tickRate1 = (byte)(tickRate % 256);
            byte tickRate2 = (byte) (tickRate / 256);
            return new byte[] { 77, 84, 104, 100, 0, 0, 0, 6, 0, 1, 0, 1, tickRate2, tickRate1 };
        }

        private byte[] StandardTrackHeader()
        {
            return new byte[] { 77, 84, 114, 107, 0, 0, 0, 255 };
        }

        private byte[] TrackEnd()
        {
            return new byte[] { 0, 255, 47, 0 };
        }

    }

}