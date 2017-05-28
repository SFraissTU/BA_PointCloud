using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudData {
    /* This class provides values for the NodeStatus of a Node, as used by the ConcurrentMultiTimeRenderer.
     * The meaning of the NodeStatus is only to be defined by the Renderer and it's the renderers responsibility to ensure the correctness.
     * Renderers do not have to use the NodeStatus, if not neccessary.
     */
    class NodeStatus {
        public const byte UNDEFINED = 0;        //Status undefined (default value)
        public const byte INVISIBLE = 1;        //Node should not be visible (outside the view frustum, below min node size, would exceed point budget...)
        public const byte TOLOAD = 2;           //Node is supposed to be loaded
        public const byte LOADING = 3;          //Node is currently loading
        public const byte TORENDER = 4;         //Node is ready for GameObject-Creation
        public const byte RENDERED = 5;         //Node is supposed to be visible and has GameObjects
        public const byte TODELETE = 6;         //Node is supposed to be invisible and its potential GameObjects have to be deleted
    }
}
