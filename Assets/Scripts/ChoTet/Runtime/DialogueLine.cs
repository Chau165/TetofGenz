using System;
using UnityEngine;

namespace ChoTet.Runtime
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea(2, 6)]
        public string text;
    }
}
