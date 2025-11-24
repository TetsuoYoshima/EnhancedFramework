// ===== Enhanced Framework - https://github.com/LucasJoestar/EnhancedFramework ===== //
// 
// Notes:
// 
// ================================================================================== //

namespace EnhancedFramework.Core {
    /// <summary>
    /// Implement this interface to be able to associate any instance of an object with a specific <see cref="Handler{T}"/>.
    /// </summary>
    public interface IHandle {
        #region Content
        /// <summary>
        /// Current id of this handle.
        /// <br/> Especially used for comparison.
        /// </summary>
        public int ID { get; }
        #endregion
    }

    /// <summary>
    /// Base <see cref="Handler{T}"/> interface.
    /// <br/> Implement it to quickly create your own <see cref="Handler{T}"/> wrappers.
    /// </summary>
    /// <typeparam name="T">Handled <see cref="IHandle"/> object type.</typeparam>
    public interface IHandler<T> : IHandle where T : IHandle {
        #region Content
        /// <summary>
        /// Get if this handler managed <see cref="IHandle"/> instance is valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Get this handler managed <see cref="IHandle"/> instance.
        /// </summary>
        /// <param name="_handle">Managed <see cref="IHandle"/> instance.</param>
        /// <returns>True if this handler managed <see cref="IHandle"/> instance is valid and could be found, false otherwise.</returns>
        bool GetHandle(out T _handle);
        #endregion
    }

    /// <summary>
    /// <see cref="IHandle"/> manager wrapper.
    /// </summary>
    /// <typeparam name="T">Handled <see cref="IHandle"/> object type.</typeparam>
    public struct Handler<T> : IHandler<T> where T : IHandle {
        #region Global Members
        private T handle;
        private int id;

        // -----------------------

        /// <inheritdoc cref="IHandle.ID"/>
        public readonly int ID { get { return id; } }

        /// <inheritdoc cref="IHandler.IsValid"/>
        public bool IsValid {
            get { return (id != 0) && (id == handle.ID); }
        }

        // -------------------------------------------
        // Constructor(s)
        // -------------------------------------------

        /// <inheritdoc cref="Handler{T}(T, int)"/>
        public Handler(T _handler) : this(_handler, _handler.ID) { }

        /// <param name="_handler">Managed <see cref="IHandle"/> instance.</param>
        /// <param name="_id"><inheritdoc cref="ID" path="/summary"/></param>
        /// <inheritdoc cref="Handler{T}"/>
        public Handler(T _handler, int _id) {
            handle = _handler;
            id     = _id;
        }
        #endregion

        #region Utility
        /// <inheritdoc cref="IHandler{T}.GetHandle(out T)"/>
        public bool GetHandle(out T _handle) {

            if (IsValid) {
                _handle = handle;
                return true;
            }

            id = 0;
            handle = default;

            _handle = default;
            return false;
        }
        #endregion
    }
}
