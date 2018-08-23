using Newtonsoft.Json;
using Protoacme.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Protoacme.Challenge
{
    public class ChallengeCollection : SerializableBase<ChallengeCollection>, IList<IAcmeChallengeContent>
    {
        private List<IAcmeChallengeContent> items = new List<IAcmeChallengeContent>();

        public ChallengeCollection() { }

        public ChallengeCollection(IEnumerable<IAcmeChallengeContent> collection)
        {
            items = new List<IAcmeChallengeContent>(collection);
        }

        public IAcmeChallengeContent this[int index] { get => items[index]; set => items[index] = value; }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public void Add(IAcmeChallengeContent item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(IAcmeChallengeContent item)
        {
            return items.Contains(item);
        }

        public void CopyTo(IAcmeChallengeContent[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IAcmeChallengeContent> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public int IndexOf(IAcmeChallengeContent item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, IAcmeChallengeContent item)
        {
            items.Insert(index, item);
        }

        public bool Remove(IAcmeChallengeContent item)
        {
            return items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }        

        public static ChallengeCollection FromBase64String<TChallenge>(string base64String)
            where TChallenge : IAcmeChallengeContent
        {
            ChallengeCollection col = new ChallengeCollection();

            if (string.IsNullOrEmpty(base64String))
                throw new ArgumentException("base64String null or empty");

            byte[] buffer = Convert.FromBase64String(base64String);
            if (buffer.Length > 0)
            {
                var o = JsonConvert.DeserializeObject<IEnumerable<TChallenge>>(Encoding.UTF8.GetString(buffer));
                col = new ChallengeCollection(o.Cast<IAcmeChallengeContent>());
            }

            return col;
        }

        public static ChallengeCollection FromFile<TChallenge>(string filePath)
            where TChallenge : IAcmeChallengeContent
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath null or empty");

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            string base64Encrypted = string.Empty;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    base64Encrypted = sr.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(base64Encrypted))
                throw new FormatException("File content is empty");

            return FromBase64String<TChallenge>(base64Encrypted);
        }
    }
}
