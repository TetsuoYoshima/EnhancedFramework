// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
//
// Notes:
//
// ================================================================================== //

using UnityEngine;

namespace EnhancedFramework.Core {
    /// <summary>
    /// Implement this interface to serialize / deserialize any data for an object during runtime.
    /// <para/> Use the <see cref="SaveManager"/> class to manipulate an object data.
    /// </summary>
    public interface ISaveable {
        #region Content
        /// <summary>
        /// Unique id used to serialize and deserialize this object data.
        /// </summary>
        public EnhancedObjectID ID { get; }

        /// <summary>
        /// Object used for logging messages.
        /// <br/> If none, you can use the <see cref="SaveManager"/>.
        /// </summary>
        public Object LogObject { get; }

        /// <summary>
        /// Serializes this object data.
        /// <br/> Use this to write and save values.
        /// </summary>
        /// <param name="_data">Use this to serialize data (don't use deserialization).</param>
        public void Serialize(ObjectSaveData _data);

        /// <summary>
        /// Deserializes this object data.
        /// <br/> Use this to load and override values.
        /// <para/>
        /// Deserialization should be performed in the same order than for serialization.
        /// </summary>
        /// <param name="_data">Use this to deserialize data (don't use serialization).</param>
        public void Deserialize(ObjectSaveData _data);
        #endregion
    }
}
