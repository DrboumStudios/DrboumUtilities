using UnityEngine;
namespace Drboum.Utilities.Runtime.Attributes {

    public class InspectorMessageAttribute : PropertyAttribute {

        public enum MessageType { Error, Info, None, Warning }

#if UNITY_EDITOR
        public string                  Message;
        public UnityEditor.MessageType Type;
        public bool                    MessageAfterProperty;

        public InspectorMessageAttribute(string message, MessageType type, bool messageAfterProperty)
        {
            Message = message;
            if ( type == MessageType.Error ) { Type   = UnityEditor.MessageType.Error; }
            if ( type == MessageType.Info ) { Type    = UnityEditor.MessageType.Info; }
            if ( type == MessageType.Warning ) { Type = UnityEditor.MessageType.Warning; }
            if ( type == MessageType.None ) { Type    = UnityEditor.MessageType.None; }
            MessageAfterProperty = messageAfterProperty;
        }

#else
		public InspectorMessageAttribute(string message, MessageType type, bool messageAfterProperty)
		{
		}
#endif
    }
}