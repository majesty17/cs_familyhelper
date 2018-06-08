using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FamilyTest
{
    class SellCommentMan
    {
        private string content;
        private string date;
        private string name;

        public string Name{
            get { return name; }
            set { name = value; }
        }
        public string Content {
            get { return content; }
            set { content = value; }
        }
        public string Date{
            get { return date; }
            set { date = value; }
        }
    }
}
