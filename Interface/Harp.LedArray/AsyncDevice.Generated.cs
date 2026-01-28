using Bonsai.Harp;
using System.Threading;
using System.Threading.Tasks;

namespace Harp.LedArray
{
    /// <inheritdoc/>
    public partial class Device
    {
        /// <summary>
        /// Initializes a new instance of the asynchronous API to configure and interface
        /// with LedArray devices on the specified serial port.
        /// </summary>
        /// <param name="portName">
        /// The name of the serial port used to communicate with the Harp device.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous initialization operation. The value of
        /// the <see cref="Task{TResult}.Result"/> parameter contains a new instance of
        /// the <see cref="AsyncDevice"/> class.
        /// </returns>
        public static async Task<AsyncDevice> CreateAsync(string portName, CancellationToken cancellationToken = default)
        {
            var device = new AsyncDevice(portName);
            var whoAmI = await device.ReadWhoAmIAsync(cancellationToken);
            if (whoAmI != Device.WhoAmI)
            {
                var errorMessage = string.Format(
                    "The device ID {1} on {0} was unexpected. Check whether a LedArray device is connected to the specified serial port.",
                    portName, whoAmI);
                throw new HarpException(errorMessage);
            }

            return device;
        }
    }

    /// <summary>
    /// Represents an asynchronous API to configure and interface with LedArray devices.
    /// </summary>
    public partial class AsyncDevice : Bonsai.Harp.AsyncDevice
    {
        internal AsyncDevice(string portName)
            : base(portName)
        {
        }

        /// <summary>
        /// Asynchronously reads the contents of the EnablePower register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<LedState> ReadEnablePowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnablePower.Address), cancellationToken);
            return EnablePower.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the EnablePower register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<LedState>> ReadTimestampedEnablePowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnablePower.Address), cancellationToken);
            return EnablePower.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the EnablePower register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteEnablePowerAsync(LedState value, CancellationToken cancellationToken = default)
        {
            var request = EnablePower.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the EnableLedMode register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<LedState> ReadEnableLedModeAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnableLedMode.Address), cancellationToken);
            return EnableLedMode.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the EnableLedMode register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<LedState>> ReadTimestampedEnableLedModeAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnableLedMode.Address), cancellationToken);
            return EnableLedMode.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the EnableLedMode register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteEnableLedModeAsync(LedState value, CancellationToken cancellationToken = default)
        {
            var request = EnableLedMode.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the EnableLed register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<LedState> ReadEnableLedAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnableLed.Address), cancellationToken);
            return EnableLed.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the EnableLed register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<LedState>> ReadTimestampedEnableLedAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnableLed.Address), cancellationToken);
            return EnableLed.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the EnableLed register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteEnableLedAsync(LedState value, CancellationToken cancellationToken = default)
        {
            var request = EnableLed.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the DigitalInputState register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<DigitalInputs> ReadDigitalInputStateAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalInputState.Address), cancellationToken);
            return DigitalInputState.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the DigitalInputState register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<DigitalInputs>> ReadTimestampedDigitalInputStateAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalInputState.Address), cancellationToken);
            return DigitalInputState.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the contents of the DigitalOutputSync register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<DigitalOutputSyncPayload> ReadDigitalOutputSyncAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalOutputSync.Address), cancellationToken);
            return DigitalOutputSync.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the DigitalOutputSync register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<DigitalOutputSyncPayload>> ReadTimestampedDigitalOutputSyncAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalOutputSync.Address), cancellationToken);
            return DigitalOutputSync.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the DigitalOutputSync register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteDigitalOutputSyncAsync(DigitalOutputSyncPayload value, CancellationToken cancellationToken = default)
        {
            var request = DigitalOutputSync.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the DigitalInputTrigger register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<DigitalInputTriggerPayload> ReadDigitalInputTriggerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalInputTrigger.Address), cancellationToken);
            return DigitalInputTrigger.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the DigitalInputTrigger register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<DigitalInputTriggerPayload>> ReadTimestampedDigitalInputTriggerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalInputTrigger.Address), cancellationToken);
            return DigitalInputTrigger.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the DigitalInputTrigger register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteDigitalInputTriggerAsync(DigitalInputTriggerPayload value, CancellationToken cancellationToken = default)
        {
            var request = DigitalInputTrigger.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the PulseMode register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<PulseModePayload> ReadPulseModeAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(PulseMode.Address), cancellationToken);
            return PulseMode.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the PulseMode register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<PulseModePayload>> ReadTimestampedPulseModeAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(PulseMode.Address), cancellationToken);
            return PulseMode.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the PulseMode register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WritePulseModeAsync(PulseModePayload value, CancellationToken cancellationToken = default)
        {
            var request = PulseMode.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0Power register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<byte> ReadLed0PowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(Led0Power.Address), cancellationToken);
            return Led0Power.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0Power register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<byte>> ReadTimestampedLed0PowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(Led0Power.Address), cancellationToken);
            return Led0Power.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0Power register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PowerAsync(byte value, CancellationToken cancellationToken = default)
        {
            var request = Led0Power.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1Power register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<byte> ReadLed1PowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(Led1Power.Address), cancellationToken);
            return Led1Power.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1Power register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<byte>> ReadTimestampedLed1PowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(Led1Power.Address), cancellationToken);
            return Led1Power.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1Power register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PowerAsync(byte value, CancellationToken cancellationToken = default)
        {
            var request = Led1Power.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PwmFrequency register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed0PwmFrequencyAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmFrequency.Address), cancellationToken);
            return Led0PwmFrequency.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PwmFrequency register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed0PwmFrequencyAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmFrequency.Address), cancellationToken);
            return Led0PwmFrequency.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PwmFrequency register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PwmFrequencyAsync(float value, CancellationToken cancellationToken = default)
        {
            var request = Led0PwmFrequency.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PwmDutyCycle register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed0PwmDutyCycleAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmDutyCycle.Address), cancellationToken);
            return Led0PwmDutyCycle.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PwmDutyCycle register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed0PwmDutyCycleAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmDutyCycle.Address), cancellationToken);
            return Led0PwmDutyCycle.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PwmDutyCycle register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PwmDutyCycleAsync(float value, CancellationToken cancellationToken = default)
        {
            var request = Led0PwmDutyCycle.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PwmPulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed0PwmPulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PwmPulseCounter.Address), cancellationToken);
            return Led0PwmPulseCounter.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PwmPulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed0PwmPulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PwmPulseCounter.Address), cancellationToken);
            return Led0PwmPulseCounter.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PwmPulseCounter register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PwmPulseCounterAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led0PwmPulseCounter.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PulseTimeOn register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed0PulseTimeOnAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimeOn.Address), cancellationToken);
            return Led0PulseTimeOn.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PulseTimeOn register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed0PulseTimeOnAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimeOn.Address), cancellationToken);
            return Led0PulseTimeOn.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PulseTimeOn register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PulseTimeOnAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led0PulseTimeOn.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PulseTimeOff register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed0PulseTimeOffAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimeOff.Address), cancellationToken);
            return Led0PulseTimeOff.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PulseTimeOff register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed0PulseTimeOffAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimeOff.Address), cancellationToken);
            return Led0PulseTimeOff.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PulseTimeOff register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PulseTimeOffAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led0PulseTimeOff.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PulseTimePulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed0PulseTimePulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimePulseCounter.Address), cancellationToken);
            return Led0PulseTimePulseCounter.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PulseTimePulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed0PulseTimePulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimePulseCounter.Address), cancellationToken);
            return Led0PulseTimePulseCounter.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PulseTimePulseCounter register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PulseTimePulseCounterAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led0PulseTimePulseCounter.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PulseTimeTail register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed0PulseTimeTailAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimeTail.Address), cancellationToken);
            return Led0PulseTimeTail.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PulseTimeTail register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed0PulseTimeTailAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseTimeTail.Address), cancellationToken);
            return Led0PulseTimeTail.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PulseTimeTail register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PulseTimeTailAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led0PulseTimeTail.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PulseRepeatCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed0PulseRepeatCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseRepeatCounter.Address), cancellationToken);
            return Led0PulseRepeatCounter.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PulseRepeatCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed0PulseRepeatCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led0PulseRepeatCounter.Address), cancellationToken);
            return Led0PulseRepeatCounter.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led0PulseRepeatCounter register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed0PulseRepeatCounterAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led0PulseRepeatCounter.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PwmFrequency register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed1PwmFrequencyAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmFrequency.Address), cancellationToken);
            return Led1PwmFrequency.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PwmFrequency register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed1PwmFrequencyAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmFrequency.Address), cancellationToken);
            return Led1PwmFrequency.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PwmFrequency register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PwmFrequencyAsync(float value, CancellationToken cancellationToken = default)
        {
            var request = Led1PwmFrequency.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PwmDutyCycle register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed1PwmDutyCycleAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmDutyCycle.Address), cancellationToken);
            return Led1PwmDutyCycle.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PwmDutyCycle register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed1PwmDutyCycleAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmDutyCycle.Address), cancellationToken);
            return Led1PwmDutyCycle.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PwmDutyCycle register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PwmDutyCycleAsync(float value, CancellationToken cancellationToken = default)
        {
            var request = Led1PwmDutyCycle.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PwmPulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed1PwmPulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PwmPulseCounter.Address), cancellationToken);
            return Led1PwmPulseCounter.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PwmPulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed1PwmPulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PwmPulseCounter.Address), cancellationToken);
            return Led1PwmPulseCounter.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PwmPulseCounter register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PwmPulseCounterAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led1PwmPulseCounter.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PulseTimeOn register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed1PulseTimeOnAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimeOn.Address), cancellationToken);
            return Led1PulseTimeOn.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PulseTimeOn register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed1PulseTimeOnAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimeOn.Address), cancellationToken);
            return Led1PulseTimeOn.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PulseTimeOn register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PulseTimeOnAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led1PulseTimeOn.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PulseTimeOff register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed1PulseTimeOffAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimeOff.Address), cancellationToken);
            return Led1PulseTimeOff.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PulseTimeOff register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed1PulseTimeOffAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimeOff.Address), cancellationToken);
            return Led1PulseTimeOff.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PulseTimeOff register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PulseTimeOffAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led1PulseTimeOff.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PulseTimePulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed1PulseTimePulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimePulseCounter.Address), cancellationToken);
            return Led1PulseTimePulseCounter.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PulseTimePulseCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed1PulseTimePulseCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimePulseCounter.Address), cancellationToken);
            return Led1PulseTimePulseCounter.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PulseTimePulseCounter register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PulseTimePulseCounterAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led1PulseTimePulseCounter.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PulseTimeTail register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed1PulseTimeTailAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimeTail.Address), cancellationToken);
            return Led1PulseTimeTail.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PulseTimeTail register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed1PulseTimeTailAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseTimeTail.Address), cancellationToken);
            return Led1PulseTimeTail.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PulseTimeTail register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PulseTimeTailAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led1PulseTimeTail.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PulseRepeatCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<ushort> ReadLed1PulseRepeatCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseRepeatCounter.Address), cancellationToken);
            return Led1PulseRepeatCounter.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PulseRepeatCounter register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<ushort>> ReadTimestampedLed1PulseRepeatCounterAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadUInt16(Led1PulseRepeatCounter.Address), cancellationToken);
            return Led1PulseRepeatCounter.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the Led1PulseRepeatCounter register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteLed1PulseRepeatCounterAsync(ushort value, CancellationToken cancellationToken = default)
        {
            var request = Led1PulseRepeatCounter.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PwmReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed0PwmRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmReal.Address), cancellationToken);
            return Led0PwmReal.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PwmReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed0PwmRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmReal.Address), cancellationToken);
            return Led0PwmReal.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led0PwmDutyCycleReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed0PwmDutyCycleRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmDutyCycleReal.Address), cancellationToken);
            return Led0PwmDutyCycleReal.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led0PwmDutyCycleReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed0PwmDutyCycleRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led0PwmDutyCycleReal.Address), cancellationToken);
            return Led0PwmDutyCycleReal.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PwmReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed1PwmRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmReal.Address), cancellationToken);
            return Led1PwmReal.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PwmReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed1PwmRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmReal.Address), cancellationToken);
            return Led1PwmReal.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the contents of the Led1PwmDutyCycleReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<float> ReadLed1PwmDutyCycleRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmDutyCycleReal.Address), cancellationToken);
            return Led1PwmDutyCycleReal.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the Led1PwmDutyCycleReal register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<float>> ReadTimestampedLed1PwmDutyCycleRealAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadSingle(Led1PwmDutyCycleReal.Address), cancellationToken);
            return Led1PwmDutyCycleReal.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the contents of the AuxDigitalOutputState register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<AuxDigitalOutputs> ReadAuxDigitalOutputStateAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(AuxDigitalOutputState.Address), cancellationToken);
            return AuxDigitalOutputState.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the AuxDigitalOutputState register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<AuxDigitalOutputs>> ReadTimestampedAuxDigitalOutputStateAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(AuxDigitalOutputState.Address), cancellationToken);
            return AuxDigitalOutputState.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the AuxDigitalOutputState register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteAuxDigitalOutputStateAsync(AuxDigitalOutputs value, CancellationToken cancellationToken = default)
        {
            var request = AuxDigitalOutputState.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the AuxLedPower register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<byte> ReadAuxLedPowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(AuxLedPower.Address), cancellationToken);
            return AuxLedPower.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the AuxLedPower register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<byte>> ReadTimestampedAuxLedPowerAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(AuxLedPower.Address), cancellationToken);
            return AuxLedPower.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the AuxLedPower register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteAuxLedPowerAsync(byte value, CancellationToken cancellationToken = default)
        {
            var request = AuxLedPower.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the DigitalOutputState register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<DigitalOutputs> ReadDigitalOutputStateAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalOutputState.Address), cancellationToken);
            return DigitalOutputState.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the DigitalOutputState register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<DigitalOutputs>> ReadTimestampedDigitalOutputStateAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(DigitalOutputState.Address), cancellationToken);
            return DigitalOutputState.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the DigitalOutputState register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteDigitalOutputStateAsync(DigitalOutputs value, CancellationToken cancellationToken = default)
        {
            var request = DigitalOutputState.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads the contents of the EnableEvents register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the register payload.
        /// </returns>
        public async Task<LedArrayEvents> ReadEnableEventsAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnableEvents.Address), cancellationToken);
            return EnableEvents.GetPayload(reply);
        }

        /// <summary>
        /// Asynchronously reads the timestamped contents of the EnableEvents register.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The <see cref="Task{TResult}.Result"/>
        /// property contains the timestamped register payload.
        /// </returns>
        public async Task<Timestamped<LedArrayEvents>> ReadTimestampedEnableEventsAsync(CancellationToken cancellationToken = default)
        {
            var reply = await CommandAsync(HarpCommand.ReadByte(EnableEvents.Address), cancellationToken);
            return EnableEvents.GetTimestampedPayload(reply);
        }

        /// <summary>
        /// Asynchronously writes a value to the EnableEvents register.
        /// </summary>
        /// <param name="value">The value to be stored in the register.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> which can be used to cancel the operation.
        /// </param>
        /// <returns>The task object representing the asynchronous write operation.</returns>
        public async Task WriteEnableEventsAsync(LedArrayEvents value, CancellationToken cancellationToken = default)
        {
            var request = EnableEvents.FromPayload(MessageType.Write, value);
            await CommandAsync(request, cancellationToken);
        }
    }
}
