namespace Drboum.Utilities.Runtime.Inputs {
    public interface IInputsData {
        void Initialize();
        /// <summary>
        ///     this method invalidate the functionality 'IsPressedThisFrame' by comparing the <see cref="CurrentTick" /> value
        ///     assigned from the param <paramref name="currentInputTick" />
        ///     and the <see cref="ButtonInput" /> internal tick  <see cref="ButtonInput.SetValue(bool, uint)" /> and
        ///     <see cref="ButtonInput.PressedThisTick" />
        /// </summary>
        /// <param name="currentInputTick">Current tick for the input IspressedThisFrame, pass 0 to disable</param>
        void Invalidate(uint currentInputTick);
        int ButtonLength { get; }
    }
}