using System.Text;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Robust.Client.UserInterface.CustomControls
{
    internal sealed class DebugCoordsPanel : PanelContainer
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IClyde _displayManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        private readonly Label _contents;
        private UIBox2i _uiBox;

        public DebugCoordsPanel()
        {
            IoCManager.InjectDependencies(this);

            HorizontalAlignment = HAlignment.Left;

            _contents = new Label
            {
                FontColorShadowOverride = Color.Black
            };
            AddChild(_contents);

            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = new Color(67, 105, 255, 138),
                ContentMarginLeftOverride = 5,
                ContentMarginTopOverride = 5
            };

            MouseFilter = _contents.MouseFilter = MouseFilterMode.Ignore;
            MinWidth = 175;
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            if (!VisibleInTree)
            {
                return;
            }

            var stringBuilder = new StringBuilder();

            var mouseScreenPos = _inputManager.MouseScreenPosition;
            var screenSize = _displayManager.ScreenSize;

            MapCoordinates mouseWorldMap;
            EntityCoordinates mouseGridPos;
            TileRef tile;

            mouseWorldMap = _eyeManager.ScreenToMap(mouseScreenPos);

            if (_mapManager.TryFindGridAt(mouseWorldMap, out var mouseGrid))
            {
                mouseGridPos = mouseGrid.MapToGrid(mouseWorldMap);
                tile = mouseGrid.GetTileRef(mouseGridPos);
            }
            else
            {
                mouseGridPos = new EntityCoordinates(_mapManager.GetMapEntityId(mouseWorldMap.MapId),
                    mouseWorldMap.Position);
                tile = new TileRef(mouseWorldMap.MapId, GridId.Invalid,
                    mouseGridPos.ToVector2i(_entityManager, _mapManager), Tile.Empty);
            }

            var controlHovered = UserInterfaceManager.CurrentlyHovered;

            stringBuilder.AppendFormat(@"Positioning Debug:
Screen Size: {0}
Mouse Pos:
    Screen: {1}
    {2}
    {3}
    {4}
    GUI: {5}", screenSize, mouseScreenPos, mouseWorldMap, mouseGridPos,
                tile, controlHovered);

            stringBuilder.AppendLine("\nAttached Entity:");
            var controlledEntity = _playerManager?.LocalPlayer?.ControlledEntity ?? EntityUid.Invalid;
            if (controlledEntity == EntityUid.Invalid)
            {
                stringBuilder.AppendLine("No attached entity.");
            }
            else
            {
                var entityTransform = _entityManager.GetComponent<TransformComponent>(controlledEntity);
                var playerWorldOffset = entityTransform.MapPosition;
                var playerScreen = _eyeManager.WorldToScreen(playerWorldOffset.Position);

                var playerCoordinates = entityTransform.Coordinates;
                var playerRotation = entityTransform.WorldRotation;

                Angle gridRotation = _mapManager.TryGetGrid(entityTransform.GridID, out var grid) ? grid.WorldRotation : Angle.Zero;

                stringBuilder.AppendFormat(@"    Screen: {0}
    {1}
    {2}
    Rotation: {3:F2}°
    EntId: {4}
    GridID: {5}
    Grid Rotation: {6:F2}°", playerScreen, playerWorldOffset, playerCoordinates, playerRotation.Degrees, entityTransform.Owner,
                    entityTransform.GridID, gridRotation.Degrees);
            }

            if (controlHovered != null)
            {
                _uiBox = UIBox2i.FromDimensions(controlHovered.GlobalPixelPosition, controlHovered.PixelSize);
            }

            _contents.Text = stringBuilder.ToString();
            // MinimumSizeChanged();
        }

        protected internal override void Draw(DrawingHandleScreen handle)
        {
            base.Draw(handle);

            if (!VisibleInTree)
            {
                return;
            }

            var (x, y) = GlobalPixelPosition;
            var renderBox = new UIBox2(
                _uiBox.Left - x,
                _uiBox.Top - y,
                _uiBox.Right - x,
                _uiBox.Bottom - y);

            handle.DrawRect(renderBox, Color.Red, false);
        }
    }
}
