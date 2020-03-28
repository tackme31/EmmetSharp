﻿using System.Collections.Generic;

namespace EmmetSharp.Models
{
    public class Node
    {
        public string Tag { get; set; }
        public string Id { get; set; }
        public ICollection<string> ClassList { get; set; }
        public IList<Node> Children { get; set; }
        public IDictionary<string, string> Attributes { get; set; }
        public string Text { get; set; }
    }
}