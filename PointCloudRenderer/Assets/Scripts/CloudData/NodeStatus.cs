using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudData {
    class NodeStatus {
        public const byte INVISIBLE = 0;
        public const byte TOLOAD = 1;
        public const byte LOADING = 2;
        public const byte TORENDER = 3;
        public const byte RENDERED = 4;
        public const byte TODELETE = 5;
    }
}
