using Robust.Shared.Map;

namespace Robust.Shared.GameObjects
{
    /// <summary>
    ///     Raised when an entity parent is changed.
    /// </summary>
    [ByRefEvent]
    public readonly struct EntParentChangedMessage
    {
        /// <summary>
        ///     Entity that was adopted. The transform component has a property with the new parent.
        /// </summary>
        public EntityUid Entity { get; }

        /// <summary>
        ///     Old parent that abandoned the Entity.
        /// </summary>
        public EntityUid? OldParent { get; }

        /// <summary>
        ///     The map Id that the entity was on before its parent changed.
        /// </summary>
        /// <remarks>
        ///     If the old parent was detached to null without manually updating the map ID of its children, then this
        ///     is required as we cannot simply use the old parent's map ID. Also avoids having to fetch the old
        ///     parent's transform component.
        /// </remarks>
        public MapId OldMapId { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="EntParentChangedMessage"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldParent"></param>
        public EntParentChangedMessage(EntityUid entity, EntityUid? oldParent, MapId oldMapId)
        {
            Entity = entity;
            OldParent = oldParent;
            OldMapId = oldMapId;
        }
    }
}
