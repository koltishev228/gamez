using UnityEngine;
using FishNet.Object.Prediction;

namespace ZombieGame.Simulation
{
    [System.Flags]
    public enum InputFlags : byte
    {
        None = 0,
        Sprint = 1 << 0,
        Crouch = 1 << 1,
        Aim = 1 << 2,
        Attack = 1 << 3,
        Interact = 1 << 4
    }

    public struct MoveInput : IReplicateData
    {
        public Vector2 Dir;
        public Vector2 Look;
        public InputFlags Flags;

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    public interface IMoveInputProvider
    {
        MoveInput GatherInput();
    }
}
