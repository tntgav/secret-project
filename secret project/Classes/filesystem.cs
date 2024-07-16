using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace secret_project
{
    public class filesystem
    {
        public Dictionary<string, filesystem> children;
        public filesystem parent;
        public string name;

        public filesystem(string name)
        {
            this.name = name;
            children = new Dictionary<string, filesystem>();
        }

        public void AddChild(filesystem child)
        {
            child.parent = this;
            children.Add(child.name, child);
        }
    }
}