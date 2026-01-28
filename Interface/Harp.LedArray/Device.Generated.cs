using Bonsai;
using Bonsai.Harp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Harp.LedArray
{
    /// <summary>
    /// Generates events and processes commands for the LedArray device connected
    /// at the specified serial port.
    /// </summary>
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("Generates events and processes commands for the LedArray device.")]
    public partial class Device : Bonsai.Harp.Device, INamedElement
    {
        /// <summary>
        /// Represents the unique identity class of the <see cref="LedArray"/> device.
        /// This field is constant.
        /// </summary>
        public const int WhoAmI = 1088;

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        public Device() : base(WhoAmI) { }

        string INamedElement.Name => nameof(LedArray);

        /// <summary>
        /// Gets a read-only mapping from address to register type.
        /// </summary>
        public static new IReadOnlyDictionary<int, Type> RegisterMap { get; } = new Dictionary<int, Type>
            (Bonsai.Harp.Device.RegisterMap.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            { 32, typeof(EnablePower) },
            { 33, typeof(EnableLedMode) },
            { 34, typeof(EnableLed) },
            { 35, typeof(DigitalInputState) },
            { 36, typeof(DigitalOutputSync) },
            { 37, typeof(DigitalInputTrigger) },
            { 38, typeof(PulseMode) },
            { 39, typeof(Led0Power) },
            { 40, typeof(Led1Power) },
            { 41, typeof(Led0PwmFrequency) },
            { 42, typeof(Led0PwmDutyCycle) },
            { 43, typeof(Led0PwmPulseCounter) },
            { 44, typeof(Led0PulseTimeOn) },
            { 45, typeof(Led0PulseTimeOff) },
            { 46, typeof(Led0PulseTimePulseCounter) },
            { 47, typeof(Led0PulseTimeTail) },
            { 48, typeof(Led0PulseRepeatCounter) },
            { 49, typeof(Led1PwmFrequency) },
            { 50, typeof(Led1PwmDutyCycle) },
            { 51, typeof(Led1PwmPulseCounter) },
            { 52, typeof(Led1PulseTimeOn) },
            { 53, typeof(Led1PulseTimeOff) },
            { 54, typeof(Led1PulseTimePulseCounter) },
            { 55, typeof(Led1PulseTimeTail) },
            { 56, typeof(Led1PulseRepeatCounter) },
            { 57, typeof(Led0PwmReal) },
            { 58, typeof(Led0PwmDutyCycleReal) },
            { 59, typeof(Led1PwmReal) },
            { 60, typeof(Led1PwmDutyCycleReal) },
            { 61, typeof(AuxDigitalOutputState) },
            { 62, typeof(AuxLedPower) },
            { 63, typeof(DigitalOutputState) },
            { 64, typeof(Reserved0) },
            { 65, typeof(EnableEvents) }
        };

        /// <summary>
        /// Gets the contents of the metadata file describing the <see cref="LedArray"/>
        /// device registers.
        /// </summary>
        public static readonly string Metadata = GetDeviceMetadata();

        static string GetDeviceMetadata()
        {
            var deviceType = typeof(Device);
            using var metadataStream = deviceType.Assembly.GetManifestResourceStream($"{deviceType.Namespace}.device.yml");
            using var streamReader = new System.IO.StreamReader(metadataStream);
            return streamReader.ReadToEnd();
        }
    }

    /// <summary>
    /// Represents an operator that returns the contents of the metadata file
    /// describing the <see cref="LedArray"/> device registers.
    /// </summary>
    [Description("Returns the contents of the metadata file describing the LedArray device registers.")]
    public partial class GetDeviceMetadata : Source<string>
    {
        /// <summary>
        /// Returns an observable sequence with the contents of the metadata file
        /// describing the <see cref="LedArray"/> device registers.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="string"/> object representing the
        /// contents of the metadata file.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Device.Metadata);
        }
    }

    /// <summary>
    /// Represents an operator that groups the sequence of <see cref="LedArray"/>" messages by register type.
    /// </summary>
    [Description("Groups the sequence of LedArray messages by register type.")]
    public partial class GroupByRegister : Combinator<HarpMessage, IGroupedObservable<Type, HarpMessage>>
    {
        /// <summary>
        /// Groups an observable sequence of <see cref="LedArray"/> messages
        /// by register type.
        /// </summary>
        /// <param name="source">The sequence of Harp device messages.</param>
        /// <returns>
        /// A sequence of observable groups, each of which corresponds to a unique
        /// <see cref="LedArray"/> register.
        /// </returns>
        public override IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<HarpMessage> source)
        {
            return source.GroupBy(message => Device.RegisterMap[message.Address]);
        }
    }

    /// <summary>
    /// Represents an operator that writes the sequence of <see cref="LedArray"/>" messages
    /// to the standard Harp storage format.
    /// </summary>
    [Description("Writes the sequence of LedArray messages to the standard Harp storage format.")]
    public partial class DeviceDataWriter : Sink<HarpMessage>, INamedElement
    {
        const string BinaryExtension = ".bin";
        const string MetadataFileName = "device.yml";
        readonly Bonsai.Harp.MessageWriter writer = new();

        string INamedElement.Name => nameof(LedArray) + "DataWriter";

        /// <summary>
        /// Gets or sets the relative or absolute path on which to save the message data.
        /// </summary>
        [Description("The relative or absolute path of the directory on which to save the message data.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path
        {
            get => System.IO.Path.GetDirectoryName(writer.FileName);
            set => writer.FileName = System.IO.Path.Combine(value, nameof(LedArray) + BinaryExtension);
        }

        /// <summary>
        /// Gets or sets a value indicating whether element writing should be buffered. If <see langword="true"/>,
        /// the write commands will be queued in memory as fast as possible and will be processed
        /// by the writer in a different thread. Otherwise, writing will be done in the same
        /// thread in which notifications arrive.
        /// </summary>
        [Description("Indicates whether writing should be buffered.")]
        public bool Buffered
        {
            get => writer.Buffered;
            set => writer.Buffered = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the output file if it already exists.
        /// </summary>
        [Description("Indicates whether to overwrite the output file if it already exists.")]
        public bool Overwrite
        {
            get => writer.Overwrite;
            set => writer.Overwrite = value;
        }

        /// <summary>
        /// Gets or sets a value specifying how the message filter will use the matching criteria.
        /// </summary>
        [Description("Specifies how the message filter will use the matching criteria.")]
        public FilterType FilterType
        {
            get => writer.FilterType;
            set => writer.FilterType = value;
        }

        /// <summary>
        /// Gets or sets a value specifying the expected message type. If no value is
        /// specified, all messages will be accepted.
        /// </summary>
        [Description("Specifies the expected message type. If no value is specified, all messages will be accepted.")]
        public MessageType? MessageType
        {
            get => writer.MessageType;
            set => writer.MessageType = value;
        }

        private IObservable<TSource> WriteDeviceMetadata<TSource>(IObservable<TSource> source)
        {
            var basePath = Path;
            if (string.IsNullOrEmpty(basePath))
                return source;

            var metadataPath = System.IO.Path.Combine(basePath, MetadataFileName);
            return Observable.Create<TSource>(observer =>
            {
                Bonsai.IO.PathHelper.EnsureDirectory(metadataPath);
                if (System.IO.File.Exists(metadataPath) && !Overwrite)
                {
                    throw new System.IO.IOException(string.Format("The file '{0}' already exists.", metadataPath));
                }

                System.IO.File.WriteAllText(metadataPath, Device.Metadata);
                return source.SubscribeSafe(observer);
            });
        }

        /// <summary>
        /// Writes each Harp message in the sequence to the specified binary file, and the
        /// contents of the device metadata file to a separate text file.
        /// </summary>
        /// <param name="source">The sequence of messages to write to the file.</param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// messages to a raw binary file, and the contents of the device metadata file
        /// to a separate text file.
        /// </returns>
        public override IObservable<HarpMessage> Process(IObservable<HarpMessage> source)
        {
            return source.Publish(ps => ps.Merge(
                WriteDeviceMetadata(writer.Process(ps.GroupBy(message => message.Address)))
                .IgnoreElements()
                .Cast<HarpMessage>()));
        }

        /// <summary>
        /// Writes each Harp message in the sequence of observable groups to the
        /// corresponding binary file, where the name of each file is generated from
        /// the common group register address. The contents of the device metadata file are
        /// written to a separate text file.
        /// </summary>
        /// <param name="source">
        /// A sequence of observable groups, each of which corresponds to a unique register
        /// address.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the Harp
        /// messages in each group to the corresponding file, and the contents of the device
        /// metadata file to a separate text file.
        /// </returns>
        public IObservable<IGroupedObservable<int, HarpMessage>> Process(IObservable<IGroupedObservable<int, HarpMessage>> source)
        {
            return WriteDeviceMetadata(writer.Process(source));
        }

        /// <summary>
        /// Writes each Harp message in the sequence of observable groups to the
        /// corresponding binary file, where the name of each file is generated from
        /// the common group register name. The contents of the device metadata file are
        /// written to a separate text file.
        /// </summary>
        /// <param name="source">
        /// A sequence of observable groups, each of which corresponds to a unique register
        /// type.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the Harp
        /// messages in each group to the corresponding file, and the contents of the device
        /// metadata file to a separate text file.
        /// </returns>
        public IObservable<IGroupedObservable<Type, HarpMessage>> Process(IObservable<IGroupedObservable<Type, HarpMessage>> source)
        {
            return WriteDeviceMetadata(writer.Process(source));
        }
    }

    /// <summary>
    /// Represents an operator that filters register-specific messages
    /// reported by the <see cref="LedArray"/> device.
    /// </summary>
    /// <seealso cref="EnablePower"/>
    /// <seealso cref="EnableLedMode"/>
    /// <seealso cref="EnableLed"/>
    /// <seealso cref="DigitalInputState"/>
    /// <seealso cref="DigitalOutputSync"/>
    /// <seealso cref="DigitalInputTrigger"/>
    /// <seealso cref="PulseMode"/>
    /// <seealso cref="Led0Power"/>
    /// <seealso cref="Led1Power"/>
    /// <seealso cref="Led0PwmFrequency"/>
    /// <seealso cref="Led0PwmDutyCycle"/>
    /// <seealso cref="Led0PwmPulseCounter"/>
    /// <seealso cref="Led0PulseTimeOn"/>
    /// <seealso cref="Led0PulseTimeOff"/>
    /// <seealso cref="Led0PulseTimePulseCounter"/>
    /// <seealso cref="Led0PulseTimeTail"/>
    /// <seealso cref="Led0PulseRepeatCounter"/>
    /// <seealso cref="Led1PwmFrequency"/>
    /// <seealso cref="Led1PwmDutyCycle"/>
    /// <seealso cref="Led1PwmPulseCounter"/>
    /// <seealso cref="Led1PulseTimeOn"/>
    /// <seealso cref="Led1PulseTimeOff"/>
    /// <seealso cref="Led1PulseTimePulseCounter"/>
    /// <seealso cref="Led1PulseTimeTail"/>
    /// <seealso cref="Led1PulseRepeatCounter"/>
    /// <seealso cref="Led0PwmReal"/>
    /// <seealso cref="Led0PwmDutyCycleReal"/>
    /// <seealso cref="Led1PwmReal"/>
    /// <seealso cref="Led1PwmDutyCycleReal"/>
    /// <seealso cref="AuxDigitalOutputState"/>
    /// <seealso cref="AuxLedPower"/>
    /// <seealso cref="DigitalOutputState"/>
    /// <seealso cref="EnableEvents"/>
    [XmlInclude(typeof(EnablePower))]
    [XmlInclude(typeof(EnableLedMode))]
    [XmlInclude(typeof(EnableLed))]
    [XmlInclude(typeof(DigitalInputState))]
    [XmlInclude(typeof(DigitalOutputSync))]
    [XmlInclude(typeof(DigitalInputTrigger))]
    [XmlInclude(typeof(PulseMode))]
    [XmlInclude(typeof(Led0Power))]
    [XmlInclude(typeof(Led1Power))]
    [XmlInclude(typeof(Led0PwmFrequency))]
    [XmlInclude(typeof(Led0PwmDutyCycle))]
    [XmlInclude(typeof(Led0PwmPulseCounter))]
    [XmlInclude(typeof(Led0PulseTimeOn))]
    [XmlInclude(typeof(Led0PulseTimeOff))]
    [XmlInclude(typeof(Led0PulseTimePulseCounter))]
    [XmlInclude(typeof(Led0PulseTimeTail))]
    [XmlInclude(typeof(Led0PulseRepeatCounter))]
    [XmlInclude(typeof(Led1PwmFrequency))]
    [XmlInclude(typeof(Led1PwmDutyCycle))]
    [XmlInclude(typeof(Led1PwmPulseCounter))]
    [XmlInclude(typeof(Led1PulseTimeOn))]
    [XmlInclude(typeof(Led1PulseTimeOff))]
    [XmlInclude(typeof(Led1PulseTimePulseCounter))]
    [XmlInclude(typeof(Led1PulseTimeTail))]
    [XmlInclude(typeof(Led1PulseRepeatCounter))]
    [XmlInclude(typeof(Led0PwmReal))]
    [XmlInclude(typeof(Led0PwmDutyCycleReal))]
    [XmlInclude(typeof(Led1PwmReal))]
    [XmlInclude(typeof(Led1PwmDutyCycleReal))]
    [XmlInclude(typeof(AuxDigitalOutputState))]
    [XmlInclude(typeof(AuxLedPower))]
    [XmlInclude(typeof(DigitalOutputState))]
    [XmlInclude(typeof(EnableEvents))]
    [Description("Filters register-specific messages reported by the LedArray device.")]
    public class FilterRegister : FilterRegisterBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterRegister"/> class.
        /// </summary>
        public FilterRegister()
        {
            Register = new EnablePower();
        }

        string INamedElement.Name
        {
            get => $"{nameof(LedArray)}.{GetElementDisplayName(Register)}";
        }
    }

    /// <summary>
    /// Represents an operator which filters and selects specific messages
    /// reported by the LedArray device.
    /// </summary>
    /// <seealso cref="EnablePower"/>
    /// <seealso cref="EnableLedMode"/>
    /// <seealso cref="EnableLed"/>
    /// <seealso cref="DigitalInputState"/>
    /// <seealso cref="DigitalOutputSync"/>
    /// <seealso cref="DigitalInputTrigger"/>
    /// <seealso cref="PulseMode"/>
    /// <seealso cref="Led0Power"/>
    /// <seealso cref="Led1Power"/>
    /// <seealso cref="Led0PwmFrequency"/>
    /// <seealso cref="Led0PwmDutyCycle"/>
    /// <seealso cref="Led0PwmPulseCounter"/>
    /// <seealso cref="Led0PulseTimeOn"/>
    /// <seealso cref="Led0PulseTimeOff"/>
    /// <seealso cref="Led0PulseTimePulseCounter"/>
    /// <seealso cref="Led0PulseTimeTail"/>
    /// <seealso cref="Led0PulseRepeatCounter"/>
    /// <seealso cref="Led1PwmFrequency"/>
    /// <seealso cref="Led1PwmDutyCycle"/>
    /// <seealso cref="Led1PwmPulseCounter"/>
    /// <seealso cref="Led1PulseTimeOn"/>
    /// <seealso cref="Led1PulseTimeOff"/>
    /// <seealso cref="Led1PulseTimePulseCounter"/>
    /// <seealso cref="Led1PulseTimeTail"/>
    /// <seealso cref="Led1PulseRepeatCounter"/>
    /// <seealso cref="Led0PwmReal"/>
    /// <seealso cref="Led0PwmDutyCycleReal"/>
    /// <seealso cref="Led1PwmReal"/>
    /// <seealso cref="Led1PwmDutyCycleReal"/>
    /// <seealso cref="AuxDigitalOutputState"/>
    /// <seealso cref="AuxLedPower"/>
    /// <seealso cref="DigitalOutputState"/>
    /// <seealso cref="EnableEvents"/>
    [XmlInclude(typeof(EnablePower))]
    [XmlInclude(typeof(EnableLedMode))]
    [XmlInclude(typeof(EnableLed))]
    [XmlInclude(typeof(DigitalInputState))]
    [XmlInclude(typeof(DigitalOutputSync))]
    [XmlInclude(typeof(DigitalInputTrigger))]
    [XmlInclude(typeof(PulseMode))]
    [XmlInclude(typeof(Led0Power))]
    [XmlInclude(typeof(Led1Power))]
    [XmlInclude(typeof(Led0PwmFrequency))]
    [XmlInclude(typeof(Led0PwmDutyCycle))]
    [XmlInclude(typeof(Led0PwmPulseCounter))]
    [XmlInclude(typeof(Led0PulseTimeOn))]
    [XmlInclude(typeof(Led0PulseTimeOff))]
    [XmlInclude(typeof(Led0PulseTimePulseCounter))]
    [XmlInclude(typeof(Led0PulseTimeTail))]
    [XmlInclude(typeof(Led0PulseRepeatCounter))]
    [XmlInclude(typeof(Led1PwmFrequency))]
    [XmlInclude(typeof(Led1PwmDutyCycle))]
    [XmlInclude(typeof(Led1PwmPulseCounter))]
    [XmlInclude(typeof(Led1PulseTimeOn))]
    [XmlInclude(typeof(Led1PulseTimeOff))]
    [XmlInclude(typeof(Led1PulseTimePulseCounter))]
    [XmlInclude(typeof(Led1PulseTimeTail))]
    [XmlInclude(typeof(Led1PulseRepeatCounter))]
    [XmlInclude(typeof(Led0PwmReal))]
    [XmlInclude(typeof(Led0PwmDutyCycleReal))]
    [XmlInclude(typeof(Led1PwmReal))]
    [XmlInclude(typeof(Led1PwmDutyCycleReal))]
    [XmlInclude(typeof(AuxDigitalOutputState))]
    [XmlInclude(typeof(AuxLedPower))]
    [XmlInclude(typeof(DigitalOutputState))]
    [XmlInclude(typeof(EnableEvents))]
    [XmlInclude(typeof(TimestampedEnablePower))]
    [XmlInclude(typeof(TimestampedEnableLedMode))]
    [XmlInclude(typeof(TimestampedEnableLed))]
    [XmlInclude(typeof(TimestampedDigitalInputState))]
    [XmlInclude(typeof(TimestampedDigitalOutputSync))]
    [XmlInclude(typeof(TimestampedDigitalInputTrigger))]
    [XmlInclude(typeof(TimestampedPulseMode))]
    [XmlInclude(typeof(TimestampedLed0Power))]
    [XmlInclude(typeof(TimestampedLed1Power))]
    [XmlInclude(typeof(TimestampedLed0PwmFrequency))]
    [XmlInclude(typeof(TimestampedLed0PwmDutyCycle))]
    [XmlInclude(typeof(TimestampedLed0PwmPulseCounter))]
    [XmlInclude(typeof(TimestampedLed0PulseTimeOn))]
    [XmlInclude(typeof(TimestampedLed0PulseTimeOff))]
    [XmlInclude(typeof(TimestampedLed0PulseTimePulseCounter))]
    [XmlInclude(typeof(TimestampedLed0PulseTimeTail))]
    [XmlInclude(typeof(TimestampedLed0PulseRepeatCounter))]
    [XmlInclude(typeof(TimestampedLed1PwmFrequency))]
    [XmlInclude(typeof(TimestampedLed1PwmDutyCycle))]
    [XmlInclude(typeof(TimestampedLed1PwmPulseCounter))]
    [XmlInclude(typeof(TimestampedLed1PulseTimeOn))]
    [XmlInclude(typeof(TimestampedLed1PulseTimeOff))]
    [XmlInclude(typeof(TimestampedLed1PulseTimePulseCounter))]
    [XmlInclude(typeof(TimestampedLed1PulseTimeTail))]
    [XmlInclude(typeof(TimestampedLed1PulseRepeatCounter))]
    [XmlInclude(typeof(TimestampedLed0PwmReal))]
    [XmlInclude(typeof(TimestampedLed0PwmDutyCycleReal))]
    [XmlInclude(typeof(TimestampedLed1PwmReal))]
    [XmlInclude(typeof(TimestampedLed1PwmDutyCycleReal))]
    [XmlInclude(typeof(TimestampedAuxDigitalOutputState))]
    [XmlInclude(typeof(TimestampedAuxLedPower))]
    [XmlInclude(typeof(TimestampedDigitalOutputState))]
    [XmlInclude(typeof(TimestampedEnableEvents))]
    [Description("Filters and selects specific messages reported by the LedArray device.")]
    public partial class Parse : ParseBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parse"/> class.
        /// </summary>
        public Parse()
        {
            Register = new EnablePower();
        }

        string INamedElement.Name => $"{nameof(LedArray)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents an operator which formats a sequence of values as specific
    /// LedArray register messages.
    /// </summary>
    /// <seealso cref="EnablePower"/>
    /// <seealso cref="EnableLedMode"/>
    /// <seealso cref="EnableLed"/>
    /// <seealso cref="DigitalInputState"/>
    /// <seealso cref="DigitalOutputSync"/>
    /// <seealso cref="DigitalInputTrigger"/>
    /// <seealso cref="PulseMode"/>
    /// <seealso cref="Led0Power"/>
    /// <seealso cref="Led1Power"/>
    /// <seealso cref="Led0PwmFrequency"/>
    /// <seealso cref="Led0PwmDutyCycle"/>
    /// <seealso cref="Led0PwmPulseCounter"/>
    /// <seealso cref="Led0PulseTimeOn"/>
    /// <seealso cref="Led0PulseTimeOff"/>
    /// <seealso cref="Led0PulseTimePulseCounter"/>
    /// <seealso cref="Led0PulseTimeTail"/>
    /// <seealso cref="Led0PulseRepeatCounter"/>
    /// <seealso cref="Led1PwmFrequency"/>
    /// <seealso cref="Led1PwmDutyCycle"/>
    /// <seealso cref="Led1PwmPulseCounter"/>
    /// <seealso cref="Led1PulseTimeOn"/>
    /// <seealso cref="Led1PulseTimeOff"/>
    /// <seealso cref="Led1PulseTimePulseCounter"/>
    /// <seealso cref="Led1PulseTimeTail"/>
    /// <seealso cref="Led1PulseRepeatCounter"/>
    /// <seealso cref="Led0PwmReal"/>
    /// <seealso cref="Led0PwmDutyCycleReal"/>
    /// <seealso cref="Led1PwmReal"/>
    /// <seealso cref="Led1PwmDutyCycleReal"/>
    /// <seealso cref="AuxDigitalOutputState"/>
    /// <seealso cref="AuxLedPower"/>
    /// <seealso cref="DigitalOutputState"/>
    /// <seealso cref="EnableEvents"/>
    [XmlInclude(typeof(EnablePower))]
    [XmlInclude(typeof(EnableLedMode))]
    [XmlInclude(typeof(EnableLed))]
    [XmlInclude(typeof(DigitalInputState))]
    [XmlInclude(typeof(DigitalOutputSync))]
    [XmlInclude(typeof(DigitalInputTrigger))]
    [XmlInclude(typeof(PulseMode))]
    [XmlInclude(typeof(Led0Power))]
    [XmlInclude(typeof(Led1Power))]
    [XmlInclude(typeof(Led0PwmFrequency))]
    [XmlInclude(typeof(Led0PwmDutyCycle))]
    [XmlInclude(typeof(Led0PwmPulseCounter))]
    [XmlInclude(typeof(Led0PulseTimeOn))]
    [XmlInclude(typeof(Led0PulseTimeOff))]
    [XmlInclude(typeof(Led0PulseTimePulseCounter))]
    [XmlInclude(typeof(Led0PulseTimeTail))]
    [XmlInclude(typeof(Led0PulseRepeatCounter))]
    [XmlInclude(typeof(Led1PwmFrequency))]
    [XmlInclude(typeof(Led1PwmDutyCycle))]
    [XmlInclude(typeof(Led1PwmPulseCounter))]
    [XmlInclude(typeof(Led1PulseTimeOn))]
    [XmlInclude(typeof(Led1PulseTimeOff))]
    [XmlInclude(typeof(Led1PulseTimePulseCounter))]
    [XmlInclude(typeof(Led1PulseTimeTail))]
    [XmlInclude(typeof(Led1PulseRepeatCounter))]
    [XmlInclude(typeof(Led0PwmReal))]
    [XmlInclude(typeof(Led0PwmDutyCycleReal))]
    [XmlInclude(typeof(Led1PwmReal))]
    [XmlInclude(typeof(Led1PwmDutyCycleReal))]
    [XmlInclude(typeof(AuxDigitalOutputState))]
    [XmlInclude(typeof(AuxLedPower))]
    [XmlInclude(typeof(DigitalOutputState))]
    [XmlInclude(typeof(EnableEvents))]
    [Description("Formats a sequence of values as specific LedArray register messages.")]
    public partial class Format : FormatBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Format"/> class.
        /// </summary>
        public Format()
        {
            Register = new EnablePower();
        }

        string INamedElement.Name => $"{nameof(LedArray)}.{GetElementDisplayName(Register)}";
    }

    /// <summary>
    /// Represents a register that control the enable of both LEDs' power supply.
    /// </summary>
    [Description("Control the enable of both LEDs' power supply.")]
    public partial class EnablePower
    {
        /// <summary>
        /// Represents the address of the <see cref="EnablePower"/> register. This field is constant.
        /// </summary>
        public const int Address = 32;

        /// <summary>
        /// Represents the payload type of the <see cref="EnablePower"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="EnablePower"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="EnablePower"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static LedState GetPayload(HarpMessage message)
        {
            return (LedState)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="EnablePower"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedState> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((LedState)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="EnablePower"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnablePower"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, LedState value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="EnablePower"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnablePower"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, LedState value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// EnablePower register.
    /// </summary>
    /// <seealso cref="EnablePower"/>
    [Description("Filters and selects timestamped messages from the EnablePower register.")]
    public partial class TimestampedEnablePower
    {
        /// <summary>
        /// Represents the address of the <see cref="EnablePower"/> register. This field is constant.
        /// </summary>
        public const int Address = EnablePower.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="EnablePower"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedState> GetPayload(HarpMessage message)
        {
            return EnablePower.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that start/stop the LEDs according to the pulse configuration.
    /// </summary>
    [Description("Start/stop the LEDs according to the pulse configuration.")]
    public partial class EnableLedMode
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableLedMode"/> register. This field is constant.
        /// </summary>
        public const int Address = 33;

        /// <summary>
        /// Represents the payload type of the <see cref="EnableLedMode"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="EnableLedMode"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="EnableLedMode"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static LedState GetPayload(HarpMessage message)
        {
            return (LedState)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="EnableLedMode"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedState> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((LedState)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="EnableLedMode"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableLedMode"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, LedState value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="EnableLedMode"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableLedMode"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, LedState value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// EnableLedMode register.
    /// </summary>
    /// <seealso cref="EnableLedMode"/>
    [Description("Filters and selects timestamped messages from the EnableLedMode register.")]
    public partial class TimestampedEnableLedMode
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableLedMode"/> register. This field is constant.
        /// </summary>
        public const int Address = EnableLedMode.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="EnableLedMode"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedState> GetPayload(HarpMessage message)
        {
            return EnableLedMode.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that enables/disables the LEDs.
    /// </summary>
    [Description("Enables/disables the LEDs.")]
    public partial class EnableLed
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableLed"/> register. This field is constant.
        /// </summary>
        public const int Address = 34;

        /// <summary>
        /// Represents the payload type of the <see cref="EnableLed"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="EnableLed"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="EnableLed"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static LedState GetPayload(HarpMessage message)
        {
            return (LedState)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="EnableLed"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedState> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((LedState)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="EnableLed"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableLed"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, LedState value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="EnableLed"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableLed"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, LedState value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// EnableLed register.
    /// </summary>
    /// <seealso cref="EnableLed"/>
    [Description("Filters and selects timestamped messages from the EnableLed register.")]
    public partial class TimestampedEnableLed
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableLed"/> register. This field is constant.
        /// </summary>
        public const int Address = EnableLed.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="EnableLed"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedState> GetPayload(HarpMessage message)
        {
            return EnableLed.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.
    /// </summary>
    [Description("State of the digital input pins. An event will be emitted when the value of any digital input pin changes.")]
    public partial class DigitalInputState
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalInputState"/> register. This field is constant.
        /// </summary>
        public const int Address = 35;

        /// <summary>
        /// Represents the payload type of the <see cref="DigitalInputState"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="DigitalInputState"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="DigitalInputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static DigitalInputs GetPayload(HarpMessage message)
        {
            return (DigitalInputs)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="DigitalInputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalInputs> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((DigitalInputs)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="DigitalInputState"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalInputState"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, DigitalInputs value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="DigitalInputState"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalInputState"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, DigitalInputs value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// DigitalInputState register.
    /// </summary>
    /// <seealso cref="DigitalInputState"/>
    [Description("Filters and selects timestamped messages from the DigitalInputState register.")]
    public partial class TimestampedDigitalInputState
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalInputState"/> register. This field is constant.
        /// </summary>
        public const int Address = DigitalInputState.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="DigitalInputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalInputs> GetPayload(HarpMessage message)
        {
            return DigitalInputState.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that configuration of the digital outputs behavior.
    /// </summary>
    [Description("Configuration of the digital outputs behavior.")]
    public partial class DigitalOutputSync
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalOutputSync"/> register. This field is constant.
        /// </summary>
        public const int Address = 36;

        /// <summary>
        /// Represents the payload type of the <see cref="DigitalOutputSync"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="DigitalOutputSync"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        static DigitalOutputSyncPayload ParsePayload(byte payload)
        {
            DigitalOutputSyncPayload result;
            result.DO0Sync = (DO0SyncConfig)(byte)(payload & 0x3);
            result.DO1Sync = (DO1SyncConfig)(byte)((payload & 0x30) >> 4);
            return result;
        }

        static byte FormatPayload(DigitalOutputSyncPayload value)
        {
            byte result;
            result = (byte)((byte)value.DO0Sync & 0x3);
            result |= (byte)(((byte)value.DO1Sync << 4) & 0x30);
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="DigitalOutputSync"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static DigitalOutputSyncPayload GetPayload(HarpMessage message)
        {
            return ParsePayload(message.GetPayloadByte());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="DigitalOutputSync"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalOutputSyncPayload> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create(ParsePayload(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="DigitalOutputSync"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalOutputSync"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, DigitalOutputSyncPayload value)
        {
            return HarpMessage.FromByte(Address, messageType, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="DigitalOutputSync"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalOutputSync"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, DigitalOutputSyncPayload value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// DigitalOutputSync register.
    /// </summary>
    /// <seealso cref="DigitalOutputSync"/>
    [Description("Filters and selects timestamped messages from the DigitalOutputSync register.")]
    public partial class TimestampedDigitalOutputSync
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalOutputSync"/> register. This field is constant.
        /// </summary>
        public const int Address = DigitalOutputSync.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="DigitalOutputSync"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalOutputSyncPayload> GetPayload(HarpMessage message)
        {
            return DigitalOutputSync.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that configuration of the digital inputs pins behavior.
    /// </summary>
    [Description("Configuration of the digital inputs pins behavior.")]
    public partial class DigitalInputTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalInputTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = 37;

        /// <summary>
        /// Represents the payload type of the <see cref="DigitalInputTrigger"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="DigitalInputTrigger"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        static DigitalInputTriggerPayload ParsePayload(byte payload)
        {
            DigitalInputTriggerPayload result;
            result.DI0Trigger = (DigitalInputTriggerConfig)(byte)(payload & 0x7);
            result.DI1Trigger = (DigitalInputTriggerConfig)(byte)((payload & 0x70) >> 4);
            return result;
        }

        static byte FormatPayload(DigitalInputTriggerPayload value)
        {
            byte result;
            result = (byte)((byte)value.DI0Trigger & 0x7);
            result |= (byte)(((byte)value.DI1Trigger << 4) & 0x70);
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="DigitalInputTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static DigitalInputTriggerPayload GetPayload(HarpMessage message)
        {
            return ParsePayload(message.GetPayloadByte());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="DigitalInputTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalInputTriggerPayload> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create(ParsePayload(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="DigitalInputTrigger"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalInputTrigger"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, DigitalInputTriggerPayload value)
        {
            return HarpMessage.FromByte(Address, messageType, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="DigitalInputTrigger"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalInputTrigger"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, DigitalInputTriggerPayload value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// DigitalInputTrigger register.
    /// </summary>
    /// <seealso cref="DigitalInputTrigger"/>
    [Description("Filters and selects timestamped messages from the DigitalInputTrigger register.")]
    public partial class TimestampedDigitalInputTrigger
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalInputTrigger"/> register. This field is constant.
        /// </summary>
        public const int Address = DigitalInputTrigger.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="DigitalInputTrigger"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalInputTriggerPayload> GetPayload(HarpMessage message)
        {
            return DigitalInputTrigger.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the pulse mode used by the LEDs.
    /// </summary>
    [Description("Sets the pulse mode used by the LEDs.")]
    public partial class PulseMode
    {
        /// <summary>
        /// Represents the address of the <see cref="PulseMode"/> register. This field is constant.
        /// </summary>
        public const int Address = 38;

        /// <summary>
        /// Represents the payload type of the <see cref="PulseMode"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="PulseMode"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        static PulseModePayload ParsePayload(byte payload)
        {
            PulseModePayload result;
            result.Led0Mode = (PulseModeConfig)(byte)(payload & 0x3);
            result.Led1Mode = (PulseModeConfig)(byte)((payload & 0x30) >> 4);
            return result;
        }

        static byte FormatPayload(PulseModePayload value)
        {
            byte result;
            result = (byte)((byte)value.Led0Mode & 0x3);
            result |= (byte)(((byte)value.Led1Mode << 4) & 0x30);
            return result;
        }

        /// <summary>
        /// Returns the payload data for <see cref="PulseMode"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static PulseModePayload GetPayload(HarpMessage message)
        {
            return ParsePayload(message.GetPayloadByte());
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="PulseMode"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<PulseModePayload> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create(ParsePayload(payload.Value), payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="PulseMode"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PulseMode"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, PulseModePayload value)
        {
            return HarpMessage.FromByte(Address, messageType, FormatPayload(value));
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="PulseMode"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="PulseMode"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, PulseModePayload value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, FormatPayload(value));
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// PulseMode register.
    /// </summary>
    /// <seealso cref="PulseMode"/>
    [Description("Filters and selects timestamped messages from the PulseMode register.")]
    public partial class TimestampedPulseMode
    {
        /// <summary>
        /// Represents the address of the <see cref="PulseMode"/> register. This field is constant.
        /// </summary>
        public const int Address = PulseMode.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="PulseMode"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<PulseModePayload> GetPayload(HarpMessage message)
        {
            return PulseMode.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the power to LED0, between 1 and 120 (arbitrary units).
    /// </summary>
    [Description("Sets the power to LED0, between 1 and 120 (arbitrary units).")]
    public partial class Led0Power
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0Power"/> register. This field is constant.
        /// </summary>
        public const int Address = 39;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0Power"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Led0Power"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0Power"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0Power"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0Power"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0Power"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0Power"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0Power"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0Power register.
    /// </summary>
    /// <seealso cref="Led0Power"/>
    [Description("Filters and selects timestamped messages from the Led0Power register.")]
    public partial class TimestampedLed0Power
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0Power"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0Power.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0Power"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return Led0Power.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the power to LED1, between 1 and 120 (arbitrary units).
    /// </summary>
    [Description("Sets the power to LED1, between 1 and 120 (arbitrary units).")]
    public partial class Led1Power
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1Power"/> register. This field is constant.
        /// </summary>
        public const int Address = 40;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1Power"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Led1Power"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1Power"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1Power"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1Power"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1Power"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1Power"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1Power"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1Power register.
    /// </summary>
    /// <seealso cref="Led1Power"/>
    [Description("Filters and selects timestamped messages from the Led1Power register.")]
    public partial class TimestampedLed1Power
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1Power"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1Power.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1Power"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return Led1Power.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.
    /// </summary>
    [Description("Sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.")]
    public partial class Led0PwmFrequency
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const int Address = 41;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led0PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PwmFrequency"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PwmFrequency"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PwmFrequency"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmFrequency"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PwmFrequency"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmFrequency"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PwmFrequency register.
    /// </summary>
    /// <seealso cref="Led0PwmFrequency"/>
    [Description("Filters and selects timestamped messages from the Led0PwmFrequency register.")]
    public partial class TimestampedLed0PwmFrequency
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PwmFrequency.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PwmFrequency"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led0PwmFrequency.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.
    /// </summary>
    [Description("Sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.")]
    public partial class Led0PwmDutyCycle
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const int Address = 42;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led0PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PwmDutyCycle"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PwmDutyCycle"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PwmDutyCycle"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmDutyCycle"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PwmDutyCycle"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmDutyCycle"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PwmDutyCycle register.
    /// </summary>
    /// <seealso cref="Led0PwmDutyCycle"/>
    [Description("Filters and selects timestamped messages from the Led0PwmDutyCycle register.")]
    public partial class TimestampedLed0PwmDutyCycle
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PwmDutyCycle.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PwmDutyCycle"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led0PwmDutyCycle.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.")]
    public partial class Led0PwmPulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = 43;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led0PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PwmPulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PwmPulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PwmPulseCounter"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmPulseCounter"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PwmPulseCounter"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmPulseCounter"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PwmPulseCounter register.
    /// </summary>
    /// <seealso cref="Led0PwmPulseCounter"/>
    [Description("Filters and selects timestamped messages from the Led0PwmPulseCounter register.")]
    public partial class TimestampedLed0PwmPulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PwmPulseCounter.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PwmPulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led0PwmPulseCounter.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led0PulseTimeOn
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const int Address = 44;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led0PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PulseTimeOn"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PulseTimeOn"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PulseTimeOn"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimeOn"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PulseTimeOn"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimeOn"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PulseTimeOn register.
    /// </summary>
    /// <seealso cref="Led0PulseTimeOn"/>
    [Description("Filters and selects timestamped messages from the Led0PulseTimeOn register.")]
    public partial class TimestampedLed0PulseTimeOn
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PulseTimeOn.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PulseTimeOn"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led0PulseTimeOn.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led0PulseTimeOff
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const int Address = 45;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led0PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PulseTimeOff"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PulseTimeOff"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PulseTimeOff"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimeOff"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PulseTimeOff"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimeOff"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PulseTimeOff register.
    /// </summary>
    /// <seealso cref="Led0PulseTimeOff"/>
    [Description("Filters and selects timestamped messages from the Led0PulseTimeOff register.")]
    public partial class TimestampedLed0PulseTimeOff
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PulseTimeOff.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PulseTimeOff"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led0PulseTimeOff.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led0PulseTimePulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = 46;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led0PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PulseTimePulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PulseTimePulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PulseTimePulseCounter"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimePulseCounter"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PulseTimePulseCounter"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimePulseCounter"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PulseTimePulseCounter register.
    /// </summary>
    /// <seealso cref="Led0PulseTimePulseCounter"/>
    [Description("Filters and selects timestamped messages from the Led0PulseTimePulseCounter register.")]
    public partial class TimestampedLed0PulseTimePulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PulseTimePulseCounter.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PulseTimePulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led0PulseTimePulseCounter.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led0PulseTimeTail
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const int Address = 47;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led0PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PulseTimeTail"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PulseTimeTail"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PulseTimeTail"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimeTail"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PulseTimeTail"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseTimeTail"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PulseTimeTail register.
    /// </summary>
    /// <seealso cref="Led0PulseTimeTail"/>
    [Description("Filters and selects timestamped messages from the Led0PulseTimeTail register.")]
    public partial class TimestampedLed0PulseTimeTail
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PulseTimeTail.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PulseTimeTail"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led0PulseTimeTail.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.")]
    public partial class Led0PulseRepeatCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = 48;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led0PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PulseRepeatCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PulseRepeatCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PulseRepeatCounter"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseRepeatCounter"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PulseRepeatCounter"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PulseRepeatCounter"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PulseRepeatCounter register.
    /// </summary>
    /// <seealso cref="Led0PulseRepeatCounter"/>
    [Description("Filters and selects timestamped messages from the Led0PulseRepeatCounter register.")]
    public partial class TimestampedLed0PulseRepeatCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PulseRepeatCounter.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PulseRepeatCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led0PulseRepeatCounter.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.
    /// </summary>
    [Description("Sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.")]
    public partial class Led1PwmFrequency
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const int Address = 49;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led1PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PwmFrequency"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PwmFrequency"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PwmFrequency"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmFrequency"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PwmFrequency"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmFrequency"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PwmFrequency register.
    /// </summary>
    /// <seealso cref="Led1PwmFrequency"/>
    [Description("Filters and selects timestamped messages from the Led1PwmFrequency register.")]
    public partial class TimestampedLed1PwmFrequency
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmFrequency"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PwmFrequency.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PwmFrequency"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led1PwmFrequency.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.
    /// </summary>
    [Description("Sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.")]
    public partial class Led1PwmDutyCycle
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const int Address = 50;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led1PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PwmDutyCycle"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PwmDutyCycle"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PwmDutyCycle"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmDutyCycle"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PwmDutyCycle"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmDutyCycle"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PwmDutyCycle register.
    /// </summary>
    /// <seealso cref="Led1PwmDutyCycle"/>
    [Description("Filters and selects timestamped messages from the Led1PwmDutyCycle register.")]
    public partial class TimestampedLed1PwmDutyCycle
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmDutyCycle"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PwmDutyCycle.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PwmDutyCycle"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led1PwmDutyCycle.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.")]
    public partial class Led1PwmPulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = 51;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led1PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PwmPulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PwmPulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PwmPulseCounter"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmPulseCounter"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PwmPulseCounter"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmPulseCounter"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PwmPulseCounter register.
    /// </summary>
    /// <seealso cref="Led1PwmPulseCounter"/>
    [Description("Filters and selects timestamped messages from the Led1PwmPulseCounter register.")]
    public partial class TimestampedLed1PwmPulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmPulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PwmPulseCounter.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PwmPulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led1PwmPulseCounter.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led1PulseTimeOn
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const int Address = 52;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led1PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PulseTimeOn"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PulseTimeOn"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PulseTimeOn"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimeOn"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PulseTimeOn"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimeOn"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PulseTimeOn register.
    /// </summary>
    /// <seealso cref="Led1PulseTimeOn"/>
    [Description("Filters and selects timestamped messages from the Led1PulseTimeOn register.")]
    public partial class TimestampedLed1PulseTimeOn
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimeOn"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PulseTimeOn.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PulseTimeOn"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led1PulseTimeOn.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led1PulseTimeOff
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const int Address = 53;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led1PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PulseTimeOff"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PulseTimeOff"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PulseTimeOff"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimeOff"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PulseTimeOff"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimeOff"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PulseTimeOff register.
    /// </summary>
    /// <seealso cref="Led1PulseTimeOff"/>
    [Description("Filters and selects timestamped messages from the Led1PulseTimeOff register.")]
    public partial class TimestampedLed1PulseTimeOff
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimeOff"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PulseTimeOff.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PulseTimeOff"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led1PulseTimeOff.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led1PulseTimePulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = 54;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led1PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PulseTimePulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PulseTimePulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PulseTimePulseCounter"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimePulseCounter"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PulseTimePulseCounter"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimePulseCounter"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PulseTimePulseCounter register.
    /// </summary>
    /// <seealso cref="Led1PulseTimePulseCounter"/>
    [Description("Filters and selects timestamped messages from the Led1PulseTimePulseCounter register.")]
    public partial class TimestampedLed1PulseTimePulseCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimePulseCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PulseTimePulseCounter.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PulseTimePulseCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led1PulseTimePulseCounter.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class Led1PulseTimeTail
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const int Address = 55;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led1PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PulseTimeTail"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PulseTimeTail"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PulseTimeTail"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimeTail"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PulseTimeTail"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseTimeTail"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PulseTimeTail register.
    /// </summary>
    /// <seealso cref="Led1PulseTimeTail"/>
    [Description("Filters and selects timestamped messages from the Led1PulseTimeTail register.")]
    public partial class TimestampedLed1PulseTimeTail
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseTimeTail"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PulseTimeTail.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PulseTimeTail"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led1PulseTimeTail.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [Description("Sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.")]
    public partial class Led1PulseRepeatCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = 56;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U16;

        /// <summary>
        /// Represents the length of the <see cref="Led1PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PulseRepeatCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static ushort GetPayload(HarpMessage message)
        {
            return message.GetPayloadUInt16();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PulseRepeatCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadUInt16();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PulseRepeatCounter"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseRepeatCounter"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PulseRepeatCounter"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PulseRepeatCounter"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, ushort value)
        {
            return HarpMessage.FromUInt16(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PulseRepeatCounter register.
    /// </summary>
    /// <seealso cref="Led1PulseRepeatCounter"/>
    [Description("Filters and selects timestamped messages from the Led1PulseRepeatCounter register.")]
    public partial class TimestampedLed1PulseRepeatCounter
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PulseRepeatCounter"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PulseRepeatCounter.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PulseRepeatCounter"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<ushort> GetPayload(HarpMessage message)
        {
            return Led1PulseRepeatCounter.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that get the real frequency (Hz) of LED0 when in Pwm mode.
    /// </summary>
    [Description("Get the real frequency (Hz) of LED0 when in Pwm mode.")]
    public partial class Led0PwmReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmReal"/> register. This field is constant.
        /// </summary>
        public const int Address = 57;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PwmReal"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led0PwmReal"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PwmReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PwmReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PwmReal"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmReal"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PwmReal"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmReal"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PwmReal register.
    /// </summary>
    /// <seealso cref="Led0PwmReal"/>
    [Description("Filters and selects timestamped messages from the Led0PwmReal register.")]
    public partial class TimestampedLed0PwmReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmReal"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PwmReal.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PwmReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led0PwmReal.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that get the real duty cycle (%) of LED0 when in Pwm mode.
    /// </summary>
    [Description("Get the real duty cycle (%) of LED0 when in Pwm mode.")]
    public partial class Led0PwmDutyCycleReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const int Address = 58;

        /// <summary>
        /// Represents the payload type of the <see cref="Led0PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led0PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led0PwmDutyCycleReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led0PwmDutyCycleReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led0PwmDutyCycleReal"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmDutyCycleReal"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led0PwmDutyCycleReal"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led0PwmDutyCycleReal"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led0PwmDutyCycleReal register.
    /// </summary>
    /// <seealso cref="Led0PwmDutyCycleReal"/>
    [Description("Filters and selects timestamped messages from the Led0PwmDutyCycleReal register.")]
    public partial class TimestampedLed0PwmDutyCycleReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led0PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const int Address = Led0PwmDutyCycleReal.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led0PwmDutyCycleReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led0PwmDutyCycleReal.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that get the real frequency (Hz) of LED1 when in Pwm mode.
    /// </summary>
    [Description("Get the real frequency (Hz) of LED1 when in Pwm mode.")]
    public partial class Led1PwmReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmReal"/> register. This field is constant.
        /// </summary>
        public const int Address = 59;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PwmReal"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led1PwmReal"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PwmReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PwmReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PwmReal"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmReal"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PwmReal"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmReal"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PwmReal register.
    /// </summary>
    /// <seealso cref="Led1PwmReal"/>
    [Description("Filters and selects timestamped messages from the Led1PwmReal register.")]
    public partial class TimestampedLed1PwmReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmReal"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PwmReal.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PwmReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led1PwmReal.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that get the real duty cycle (%) of LED1 when in Pwm mode.
    /// </summary>
    [Description("Get the real duty cycle (%) of LED1 when in Pwm mode.")]
    public partial class Led1PwmDutyCycleReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const int Address = 60;

        /// <summary>
        /// Represents the payload type of the <see cref="Led1PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.Float;

        /// <summary>
        /// Represents the length of the <see cref="Led1PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="Led1PwmDutyCycleReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static float GetPayload(HarpMessage message)
        {
            return message.GetPayloadSingle();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="Led1PwmDutyCycleReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadSingle();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="Led1PwmDutyCycleReal"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmDutyCycleReal"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="Led1PwmDutyCycleReal"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="Led1PwmDutyCycleReal"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, float value)
        {
            return HarpMessage.FromSingle(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// Led1PwmDutyCycleReal register.
    /// </summary>
    /// <seealso cref="Led1PwmDutyCycleReal"/>
    [Description("Filters and selects timestamped messages from the Led1PwmDutyCycleReal register.")]
    public partial class TimestampedLed1PwmDutyCycleReal
    {
        /// <summary>
        /// Represents the address of the <see cref="Led1PwmDutyCycleReal"/> register. This field is constant.
        /// </summary>
        public const int Address = Led1PwmDutyCycleReal.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="Led1PwmDutyCycleReal"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<float> GetPayload(HarpMessage message)
        {
            return Led1PwmDutyCycleReal.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that write the state of the auxiliary digital output bit.
    /// </summary>
    [Description("Write the state of the auxiliary digital output bit.")]
    public partial class AuxDigitalOutputState
    {
        /// <summary>
        /// Represents the address of the <see cref="AuxDigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const int Address = 61;

        /// <summary>
        /// Represents the payload type of the <see cref="AuxDigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="AuxDigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="AuxDigitalOutputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static AuxDigitalOutputs GetPayload(HarpMessage message)
        {
            return (AuxDigitalOutputs)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="AuxDigitalOutputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<AuxDigitalOutputs> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((AuxDigitalOutputs)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="AuxDigitalOutputState"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AuxDigitalOutputState"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, AuxDigitalOutputs value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="AuxDigitalOutputState"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AuxDigitalOutputState"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, AuxDigitalOutputs value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// AuxDigitalOutputState register.
    /// </summary>
    /// <seealso cref="AuxDigitalOutputState"/>
    [Description("Filters and selects timestamped messages from the AuxDigitalOutputState register.")]
    public partial class TimestampedAuxDigitalOutputState
    {
        /// <summary>
        /// Represents the address of the <see cref="AuxDigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const int Address = AuxDigitalOutputState.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="AuxDigitalOutputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<AuxDigitalOutputs> GetPayload(HarpMessage message)
        {
            return AuxDigitalOutputState.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that sets the power to be applied to auxiliary LED, between 1 and 120.
    /// </summary>
    [Description("Sets the power to be applied to auxiliary LED, between 1 and 120.")]
    public partial class AuxLedPower
    {
        /// <summary>
        /// Represents the address of the <see cref="AuxLedPower"/> register. This field is constant.
        /// </summary>
        public const int Address = 62;

        /// <summary>
        /// Represents the payload type of the <see cref="AuxLedPower"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="AuxLedPower"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="AuxLedPower"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static byte GetPayload(HarpMessage message)
        {
            return message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="AuxLedPower"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetTimestampedPayload(HarpMessage message)
        {
            return message.GetTimestampedPayloadByte();
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="AuxLedPower"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AuxLedPower"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, messageType, value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="AuxLedPower"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="AuxLedPower"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, byte value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// AuxLedPower register.
    /// </summary>
    /// <seealso cref="AuxLedPower"/>
    [Description("Filters and selects timestamped messages from the AuxLedPower register.")]
    public partial class TimestampedAuxLedPower
    {
        /// <summary>
        /// Represents the address of the <see cref="AuxLedPower"/> register. This field is constant.
        /// </summary>
        public const int Address = AuxLedPower.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="AuxLedPower"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<byte> GetPayload(HarpMessage message)
        {
            return AuxLedPower.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that write the state of digital output lines.
    /// </summary>
    [Description("Write the state of digital output lines.")]
    public partial class DigitalOutputState
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const int Address = 63;

        /// <summary>
        /// Represents the payload type of the <see cref="DigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="DigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="DigitalOutputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static DigitalOutputs GetPayload(HarpMessage message)
        {
            return (DigitalOutputs)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="DigitalOutputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalOutputs> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((DigitalOutputs)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="DigitalOutputState"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalOutputState"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, DigitalOutputs value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="DigitalOutputState"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="DigitalOutputState"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, DigitalOutputs value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// DigitalOutputState register.
    /// </summary>
    /// <seealso cref="DigitalOutputState"/>
    [Description("Filters and selects timestamped messages from the DigitalOutputState register.")]
    public partial class TimestampedDigitalOutputState
    {
        /// <summary>
        /// Represents the address of the <see cref="DigitalOutputState"/> register. This field is constant.
        /// </summary>
        public const int Address = DigitalOutputState.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="DigitalOutputState"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<DigitalOutputs> GetPayload(HarpMessage message)
        {
            return DigitalOutputState.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents a register that reserved for future use.
    /// </summary>
    [Description("Reserved for future use")]
    internal partial class Reserved0
    {
        /// <summary>
        /// Represents the address of the <see cref="Reserved0"/> register. This field is constant.
        /// </summary>
        public const int Address = 64;

        /// <summary>
        /// Represents the payload type of the <see cref="Reserved0"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="Reserved0"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;
    }

    /// <summary>
    /// Represents a register that specifies all the active events in the device.
    /// </summary>
    [Description("Specifies all the active events in the device.")]
    public partial class EnableEvents
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const int Address = 65;

        /// <summary>
        /// Represents the payload type of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const PayloadType RegisterType = PayloadType.U8;

        /// <summary>
        /// Represents the length of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const int RegisterLength = 1;

        /// <summary>
        /// Returns the payload data for <see cref="EnableEvents"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the message payload.</returns>
        public static LedArrayEvents GetPayload(HarpMessage message)
        {
            return (LedArrayEvents)message.GetPayloadByte();
        }

        /// <summary>
        /// Returns the timestamped payload data for <see cref="EnableEvents"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedArrayEvents> GetTimestampedPayload(HarpMessage message)
        {
            var payload = message.GetTimestampedPayloadByte();
            return Timestamped.Create((LedArrayEvents)payload.Value, payload.Seconds);
        }

        /// <summary>
        /// Returns a Harp message for the <see cref="EnableEvents"/> register.
        /// </summary>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableEvents"/> register
        /// with the specified message type and payload.
        /// </returns>
        public static HarpMessage FromPayload(MessageType messageType, LedArrayEvents value)
        {
            return HarpMessage.FromByte(Address, messageType, (byte)value);
        }

        /// <summary>
        /// Returns a timestamped Harp message for the <see cref="EnableEvents"/>
        /// register.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">The type of the Harp message.</param>
        /// <param name="value">The value to be stored in the message payload.</param>
        /// <returns>
        /// A <see cref="HarpMessage"/> object for the <see cref="EnableEvents"/> register
        /// with the specified message type, timestamp, and payload.
        /// </returns>
        public static HarpMessage FromPayload(double timestamp, MessageType messageType, LedArrayEvents value)
        {
            return HarpMessage.FromByte(Address, timestamp, messageType, (byte)value);
        }
    }

    /// <summary>
    /// Provides methods for manipulating timestamped messages from the
    /// EnableEvents register.
    /// </summary>
    /// <seealso cref="EnableEvents"/>
    [Description("Filters and selects timestamped messages from the EnableEvents register.")]
    public partial class TimestampedEnableEvents
    {
        /// <summary>
        /// Represents the address of the <see cref="EnableEvents"/> register. This field is constant.
        /// </summary>
        public const int Address = EnableEvents.Address;

        /// <summary>
        /// Returns timestamped payload data for <see cref="EnableEvents"/> register messages.
        /// </summary>
        /// <param name="message">A <see cref="HarpMessage"/> object representing the register message.</param>
        /// <returns>A value representing the timestamped message payload.</returns>
        public static Timestamped<LedArrayEvents> GetPayload(HarpMessage message)
        {
            return EnableEvents.GetTimestampedPayload(message);
        }
    }

    /// <summary>
    /// Represents an operator which creates standard message payloads for the
    /// LedArray device.
    /// </summary>
    /// <seealso cref="CreateEnablePowerPayload"/>
    /// <seealso cref="CreateEnableLedModePayload"/>
    /// <seealso cref="CreateEnableLedPayload"/>
    /// <seealso cref="CreateDigitalInputStatePayload"/>
    /// <seealso cref="CreateDigitalOutputSyncPayload"/>
    /// <seealso cref="CreateDigitalInputTriggerPayload"/>
    /// <seealso cref="CreatePulseModePayload"/>
    /// <seealso cref="CreateLed0PowerPayload"/>
    /// <seealso cref="CreateLed1PowerPayload"/>
    /// <seealso cref="CreateLed0PwmFrequencyPayload"/>
    /// <seealso cref="CreateLed0PwmDutyCyclePayload"/>
    /// <seealso cref="CreateLed0PwmPulseCounterPayload"/>
    /// <seealso cref="CreateLed0PulseTimeOnPayload"/>
    /// <seealso cref="CreateLed0PulseTimeOffPayload"/>
    /// <seealso cref="CreateLed0PulseTimePulseCounterPayload"/>
    /// <seealso cref="CreateLed0PulseTimeTailPayload"/>
    /// <seealso cref="CreateLed0PulseRepeatCounterPayload"/>
    /// <seealso cref="CreateLed1PwmFrequencyPayload"/>
    /// <seealso cref="CreateLed1PwmDutyCyclePayload"/>
    /// <seealso cref="CreateLed1PwmPulseCounterPayload"/>
    /// <seealso cref="CreateLed1PulseTimeOnPayload"/>
    /// <seealso cref="CreateLed1PulseTimeOffPayload"/>
    /// <seealso cref="CreateLed1PulseTimePulseCounterPayload"/>
    /// <seealso cref="CreateLed1PulseTimeTailPayload"/>
    /// <seealso cref="CreateLed1PulseRepeatCounterPayload"/>
    /// <seealso cref="CreateLed0PwmRealPayload"/>
    /// <seealso cref="CreateLed0PwmDutyCycleRealPayload"/>
    /// <seealso cref="CreateLed1PwmRealPayload"/>
    /// <seealso cref="CreateLed1PwmDutyCycleRealPayload"/>
    /// <seealso cref="CreateAuxDigitalOutputStatePayload"/>
    /// <seealso cref="CreateAuxLedPowerPayload"/>
    /// <seealso cref="CreateDigitalOutputStatePayload"/>
    /// <seealso cref="CreateEnableEventsPayload"/>
    [XmlInclude(typeof(CreateEnablePowerPayload))]
    [XmlInclude(typeof(CreateEnableLedModePayload))]
    [XmlInclude(typeof(CreateEnableLedPayload))]
    [XmlInclude(typeof(CreateDigitalInputStatePayload))]
    [XmlInclude(typeof(CreateDigitalOutputSyncPayload))]
    [XmlInclude(typeof(CreateDigitalInputTriggerPayload))]
    [XmlInclude(typeof(CreatePulseModePayload))]
    [XmlInclude(typeof(CreateLed0PowerPayload))]
    [XmlInclude(typeof(CreateLed1PowerPayload))]
    [XmlInclude(typeof(CreateLed0PwmFrequencyPayload))]
    [XmlInclude(typeof(CreateLed0PwmDutyCyclePayload))]
    [XmlInclude(typeof(CreateLed0PwmPulseCounterPayload))]
    [XmlInclude(typeof(CreateLed0PulseTimeOnPayload))]
    [XmlInclude(typeof(CreateLed0PulseTimeOffPayload))]
    [XmlInclude(typeof(CreateLed0PulseTimePulseCounterPayload))]
    [XmlInclude(typeof(CreateLed0PulseTimeTailPayload))]
    [XmlInclude(typeof(CreateLed0PulseRepeatCounterPayload))]
    [XmlInclude(typeof(CreateLed1PwmFrequencyPayload))]
    [XmlInclude(typeof(CreateLed1PwmDutyCyclePayload))]
    [XmlInclude(typeof(CreateLed1PwmPulseCounterPayload))]
    [XmlInclude(typeof(CreateLed1PulseTimeOnPayload))]
    [XmlInclude(typeof(CreateLed1PulseTimeOffPayload))]
    [XmlInclude(typeof(CreateLed1PulseTimePulseCounterPayload))]
    [XmlInclude(typeof(CreateLed1PulseTimeTailPayload))]
    [XmlInclude(typeof(CreateLed1PulseRepeatCounterPayload))]
    [XmlInclude(typeof(CreateLed0PwmRealPayload))]
    [XmlInclude(typeof(CreateLed0PwmDutyCycleRealPayload))]
    [XmlInclude(typeof(CreateLed1PwmRealPayload))]
    [XmlInclude(typeof(CreateLed1PwmDutyCycleRealPayload))]
    [XmlInclude(typeof(CreateAuxDigitalOutputStatePayload))]
    [XmlInclude(typeof(CreateAuxLedPowerPayload))]
    [XmlInclude(typeof(CreateDigitalOutputStatePayload))]
    [XmlInclude(typeof(CreateEnableEventsPayload))]
    [XmlInclude(typeof(CreateTimestampedEnablePowerPayload))]
    [XmlInclude(typeof(CreateTimestampedEnableLedModePayload))]
    [XmlInclude(typeof(CreateTimestampedEnableLedPayload))]
    [XmlInclude(typeof(CreateTimestampedDigitalInputStatePayload))]
    [XmlInclude(typeof(CreateTimestampedDigitalOutputSyncPayload))]
    [XmlInclude(typeof(CreateTimestampedDigitalInputTriggerPayload))]
    [XmlInclude(typeof(CreateTimestampedPulseModePayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PowerPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PowerPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PwmFrequencyPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PwmDutyCyclePayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PwmPulseCounterPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PulseTimeOnPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PulseTimeOffPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PulseTimePulseCounterPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PulseTimeTailPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PulseRepeatCounterPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PwmFrequencyPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PwmDutyCyclePayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PwmPulseCounterPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PulseTimeOnPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PulseTimeOffPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PulseTimePulseCounterPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PulseTimeTailPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PulseRepeatCounterPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PwmRealPayload))]
    [XmlInclude(typeof(CreateTimestampedLed0PwmDutyCycleRealPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PwmRealPayload))]
    [XmlInclude(typeof(CreateTimestampedLed1PwmDutyCycleRealPayload))]
    [XmlInclude(typeof(CreateTimestampedAuxDigitalOutputStatePayload))]
    [XmlInclude(typeof(CreateTimestampedAuxLedPowerPayload))]
    [XmlInclude(typeof(CreateTimestampedDigitalOutputStatePayload))]
    [XmlInclude(typeof(CreateTimestampedEnableEventsPayload))]
    [Description("Creates standard message payloads for the LedArray device.")]
    public partial class CreateMessage : CreateMessageBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateMessage"/> class.
        /// </summary>
        public CreateMessage()
        {
            Payload = new CreateEnablePowerPayload();
        }

        string INamedElement.Name => $"{nameof(LedArray)}.{GetElementDisplayName(Payload)}";
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that control the enable of both LEDs' power supply.
    /// </summary>
    [DisplayName("EnablePowerPayload")]
    [Description("Creates a message payload that control the enable of both LEDs' power supply.")]
    public partial class CreateEnablePowerPayload
    {
        /// <summary>
        /// Gets or sets the value that control the enable of both LEDs' power supply.
        /// </summary>
        [Description("The value that control the enable of both LEDs' power supply.")]
        public LedState EnablePower { get; set; }

        /// <summary>
        /// Creates a message payload for the EnablePower register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public LedState GetPayload()
        {
            return EnablePower;
        }

        /// <summary>
        /// Creates a message that control the enable of both LEDs' power supply.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the EnablePower register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.EnablePower.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that control the enable of both LEDs' power supply.
    /// </summary>
    [DisplayName("TimestampedEnablePowerPayload")]
    [Description("Creates a timestamped message payload that control the enable of both LEDs' power supply.")]
    public partial class CreateTimestampedEnablePowerPayload : CreateEnablePowerPayload
    {
        /// <summary>
        /// Creates a timestamped message that control the enable of both LEDs' power supply.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the EnablePower register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.EnablePower.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that start/stop the LEDs according to the pulse configuration.
    /// </summary>
    [DisplayName("EnableLedModePayload")]
    [Description("Creates a message payload that start/stop the LEDs according to the pulse configuration.")]
    public partial class CreateEnableLedModePayload
    {
        /// <summary>
        /// Gets or sets the value that start/stop the LEDs according to the pulse configuration.
        /// </summary>
        [Description("The value that start/stop the LEDs according to the pulse configuration.")]
        public LedState EnableLedMode { get; set; }

        /// <summary>
        /// Creates a message payload for the EnableLedMode register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public LedState GetPayload()
        {
            return EnableLedMode;
        }

        /// <summary>
        /// Creates a message that start/stop the LEDs according to the pulse configuration.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the EnableLedMode register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.EnableLedMode.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that start/stop the LEDs according to the pulse configuration.
    /// </summary>
    [DisplayName("TimestampedEnableLedModePayload")]
    [Description("Creates a timestamped message payload that start/stop the LEDs according to the pulse configuration.")]
    public partial class CreateTimestampedEnableLedModePayload : CreateEnableLedModePayload
    {
        /// <summary>
        /// Creates a timestamped message that start/stop the LEDs according to the pulse configuration.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the EnableLedMode register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.EnableLedMode.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that enables/disables the LEDs.
    /// </summary>
    [DisplayName("EnableLedPayload")]
    [Description("Creates a message payload that enables/disables the LEDs.")]
    public partial class CreateEnableLedPayload
    {
        /// <summary>
        /// Gets or sets the value that enables/disables the LEDs.
        /// </summary>
        [Description("The value that enables/disables the LEDs.")]
        public LedState EnableLed { get; set; }

        /// <summary>
        /// Creates a message payload for the EnableLed register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public LedState GetPayload()
        {
            return EnableLed;
        }

        /// <summary>
        /// Creates a message that enables/disables the LEDs.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the EnableLed register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.EnableLed.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that enables/disables the LEDs.
    /// </summary>
    [DisplayName("TimestampedEnableLedPayload")]
    [Description("Creates a timestamped message payload that enables/disables the LEDs.")]
    public partial class CreateTimestampedEnableLedPayload : CreateEnableLedPayload
    {
        /// <summary>
        /// Creates a timestamped message that enables/disables the LEDs.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the EnableLed register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.EnableLed.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.
    /// </summary>
    [DisplayName("DigitalInputStatePayload")]
    [Description("Creates a message payload that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.")]
    public partial class CreateDigitalInputStatePayload
    {
        /// <summary>
        /// Gets or sets the value that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.
        /// </summary>
        [Description("The value that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.")]
        public DigitalInputs DigitalInputState { get; set; }

        /// <summary>
        /// Creates a message payload for the DigitalInputState register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public DigitalInputs GetPayload()
        {
            return DigitalInputState;
        }

        /// <summary>
        /// Creates a message that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the DigitalInputState register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.DigitalInputState.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.
    /// </summary>
    [DisplayName("TimestampedDigitalInputStatePayload")]
    [Description("Creates a timestamped message payload that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.")]
    public partial class CreateTimestampedDigitalInputStatePayload : CreateDigitalInputStatePayload
    {
        /// <summary>
        /// Creates a timestamped message that state of the digital input pins. An event will be emitted when the value of any digital input pin changes.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the DigitalInputState register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.DigitalInputState.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that configuration of the digital outputs behavior.
    /// </summary>
    [DisplayName("DigitalOutputSyncPayload")]
    [Description("Creates a message payload that configuration of the digital outputs behavior.")]
    public partial class CreateDigitalOutputSyncPayload
    {
        /// <summary>
        /// Gets or sets a value that configuration of the DO0 functionality.
        /// </summary>
        [Description("Configuration of the DO0 functionality.")]
        public DO0SyncConfig DO0Sync { get; set; }

        /// <summary>
        /// Gets or sets a value that configuration of the DO1 functionality.
        /// </summary>
        [Description("Configuration of the DO1 functionality.")]
        public DO1SyncConfig DO1Sync { get; set; }

        /// <summary>
        /// Creates a message payload for the DigitalOutputSync register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public DigitalOutputSyncPayload GetPayload()
        {
            DigitalOutputSyncPayload value;
            value.DO0Sync = DO0Sync;
            value.DO1Sync = DO1Sync;
            return value;
        }

        /// <summary>
        /// Creates a message that configuration of the digital outputs behavior.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the DigitalOutputSync register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.DigitalOutputSync.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that configuration of the digital outputs behavior.
    /// </summary>
    [DisplayName("TimestampedDigitalOutputSyncPayload")]
    [Description("Creates a timestamped message payload that configuration of the digital outputs behavior.")]
    public partial class CreateTimestampedDigitalOutputSyncPayload : CreateDigitalOutputSyncPayload
    {
        /// <summary>
        /// Creates a timestamped message that configuration of the digital outputs behavior.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the DigitalOutputSync register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.DigitalOutputSync.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that configuration of the digital inputs pins behavior.
    /// </summary>
    [DisplayName("DigitalInputTriggerPayload")]
    [Description("Creates a message payload that configuration of the digital inputs pins behavior.")]
    public partial class CreateDigitalInputTriggerPayload
    {
        /// <summary>
        /// Gets or sets a value that configuration of the DIO input pin.
        /// </summary>
        [Description("Configuration of the DIO input pin.")]
        public DigitalInputTriggerConfig DI0Trigger { get; set; }

        /// <summary>
        /// Gets or sets a value that configuration of the DI1 input pin.
        /// </summary>
        [Description("Configuration of the DI1 input pin.")]
        public DigitalInputTriggerConfig DI1Trigger { get; set; }

        /// <summary>
        /// Creates a message payload for the DigitalInputTrigger register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public DigitalInputTriggerPayload GetPayload()
        {
            DigitalInputTriggerPayload value;
            value.DI0Trigger = DI0Trigger;
            value.DI1Trigger = DI1Trigger;
            return value;
        }

        /// <summary>
        /// Creates a message that configuration of the digital inputs pins behavior.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the DigitalInputTrigger register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.DigitalInputTrigger.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that configuration of the digital inputs pins behavior.
    /// </summary>
    [DisplayName("TimestampedDigitalInputTriggerPayload")]
    [Description("Creates a timestamped message payload that configuration of the digital inputs pins behavior.")]
    public partial class CreateTimestampedDigitalInputTriggerPayload : CreateDigitalInputTriggerPayload
    {
        /// <summary>
        /// Creates a timestamped message that configuration of the digital inputs pins behavior.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the DigitalInputTrigger register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.DigitalInputTrigger.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the pulse mode used by the LEDs.
    /// </summary>
    [DisplayName("PulseModePayload")]
    [Description("Creates a message payload that sets the pulse mode used by the LEDs.")]
    public partial class CreatePulseModePayload
    {
        /// <summary>
        /// Gets or sets a value that sets the pulse mode used in LED0.
        /// </summary>
        [Description("Sets the pulse mode used in LED0.")]
        public PulseModeConfig Led0Mode { get; set; }

        /// <summary>
        /// Gets or sets a value that sets the pulse mode used in LED0.
        /// </summary>
        [Description("Sets the pulse mode used in LED0")]
        public PulseModeConfig Led1Mode { get; set; }

        /// <summary>
        /// Creates a message payload for the PulseMode register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public PulseModePayload GetPayload()
        {
            PulseModePayload value;
            value.Led0Mode = Led0Mode;
            value.Led1Mode = Led1Mode;
            return value;
        }

        /// <summary>
        /// Creates a message that sets the pulse mode used by the LEDs.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the PulseMode register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.PulseMode.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the pulse mode used by the LEDs.
    /// </summary>
    [DisplayName("TimestampedPulseModePayload")]
    [Description("Creates a timestamped message payload that sets the pulse mode used by the LEDs.")]
    public partial class CreateTimestampedPulseModePayload : CreatePulseModePayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the pulse mode used by the LEDs.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the PulseMode register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.PulseMode.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the power to LED0, between 1 and 120 (arbitrary units).
    /// </summary>
    [DisplayName("Led0PowerPayload")]
    [Description("Creates a message payload that sets the power to LED0, between 1 and 120 (arbitrary units).")]
    public partial class CreateLed0PowerPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the power to LED0, between 1 and 120 (arbitrary units).
        /// </summary>
        [Range(min: 1, max: 120)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the power to LED0, between 1 and 120 (arbitrary units).")]
        public byte Led0Power { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0Power register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return Led0Power;
        }

        /// <summary>
        /// Creates a message that sets the power to LED0, between 1 and 120 (arbitrary units).
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0Power register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0Power.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the power to LED0, between 1 and 120 (arbitrary units).
    /// </summary>
    [DisplayName("TimestampedLed0PowerPayload")]
    [Description("Creates a timestamped message payload that sets the power to LED0, between 1 and 120 (arbitrary units).")]
    public partial class CreateTimestampedLed0PowerPayload : CreateLed0PowerPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the power to LED0, between 1 and 120 (arbitrary units).
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0Power register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0Power.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the power to LED1, between 1 and 120 (arbitrary units).
    /// </summary>
    [DisplayName("Led1PowerPayload")]
    [Description("Creates a message payload that sets the power to LED1, between 1 and 120 (arbitrary units).")]
    public partial class CreateLed1PowerPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the power to LED1, between 1 and 120 (arbitrary units).
        /// </summary>
        [Range(min: 1, max: 120)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the power to LED1, between 1 and 120 (arbitrary units).")]
        public byte Led1Power { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1Power register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return Led1Power;
        }

        /// <summary>
        /// Creates a message that sets the power to LED1, between 1 and 120 (arbitrary units).
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1Power register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1Power.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the power to LED1, between 1 and 120 (arbitrary units).
    /// </summary>
    [DisplayName("TimestampedLed1PowerPayload")]
    [Description("Creates a timestamped message payload that sets the power to LED1, between 1 and 120 (arbitrary units).")]
    public partial class CreateTimestampedLed1PowerPayload : CreateLed1PowerPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the power to LED1, between 1 and 120 (arbitrary units).
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1Power register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1Power.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.
    /// </summary>
    [DisplayName("Led0PwmFrequencyPayload")]
    [Description("Creates a message payload that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.")]
    public partial class CreateLed0PwmFrequencyPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.
        /// </summary>
        [Range(min: 0.5, max: 2000)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.")]
        public float Led0PwmFrequency { get; set; } = 0.5F;

        /// <summary>
        /// Creates a message payload for the Led0PwmFrequency register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led0PwmFrequency;
        }

        /// <summary>
        /// Creates a message that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PwmFrequency register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PwmFrequency.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.
    /// </summary>
    [DisplayName("TimestampedLed0PwmFrequencyPayload")]
    [Description("Creates a timestamped message payload that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.")]
    public partial class CreateTimestampedLed0PwmFrequencyPayload : CreateLed0PwmFrequencyPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the frequency (Hz) of LED0 when in Pwm mode, between 0.5 and 2000.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PwmFrequency register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PwmFrequency.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.
    /// </summary>
    [DisplayName("Led0PwmDutyCyclePayload")]
    [Description("Creates a message payload that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.")]
    public partial class CreateLed0PwmDutyCyclePayload
    {
        /// <summary>
        /// Gets or sets the value that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.
        /// </summary>
        [Range(min: 0.1, max: 99.9)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.")]
        public float Led0PwmDutyCycle { get; set; } = 0.1F;

        /// <summary>
        /// Creates a message payload for the Led0PwmDutyCycle register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led0PwmDutyCycle;
        }

        /// <summary>
        /// Creates a message that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PwmDutyCycle register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PwmDutyCycle.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.
    /// </summary>
    [DisplayName("TimestampedLed0PwmDutyCyclePayload")]
    [Description("Creates a timestamped message payload that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.")]
    public partial class CreateTimestampedLed0PwmDutyCyclePayload : CreateLed0PwmDutyCyclePayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the duty cycle (%) of LED0 when in Pwm mode, between 0.1 and 99.9.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PwmDutyCycle register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PwmDutyCycle.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led0PwmPulseCounterPayload")]
    [Description("Creates a message payload that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.")]
    public partial class CreateLed0PwmPulseCounterPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.")]
        public ushort Led0PwmPulseCounter { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0PwmPulseCounter register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led0PwmPulseCounter;
        }

        /// <summary>
        /// Creates a message that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PwmPulseCounter register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PwmPulseCounter.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed0PwmPulseCounterPayload")]
    [Description("Creates a timestamped message payload that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed0PwmPulseCounterPayload : CreateLed0PwmPulseCounterPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the number of pulses of LED0 when in Pwm mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PwmPulseCounter register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PwmPulseCounter.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led0PulseTimeOnPayload")]
    [Description("Creates a message payload that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed0PulseTimeOnPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led0PulseTimeOn { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0PulseTimeOn register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led0PulseTimeOn;
        }

        /// <summary>
        /// Creates a message that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PulseTimeOn register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimeOn.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed0PulseTimeOnPayload")]
    [Description("Creates a timestamped message payload that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed0PulseTimeOnPayload : CreateLed0PulseTimeOnPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the time on (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PulseTimeOn register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimeOn.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led0PulseTimeOffPayload")]
    [Description("Creates a message payload that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed0PulseTimeOffPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led0PulseTimeOff { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0PulseTimeOff register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led0PulseTimeOff;
        }

        /// <summary>
        /// Creates a message that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PulseTimeOff register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimeOff.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed0PulseTimeOffPayload")]
    [Description("Creates a timestamped message payload that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed0PulseTimeOffPayload : CreateLed0PulseTimeOffPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the time off (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PulseTimeOff register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimeOff.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led0PulseTimePulseCounterPayload")]
    [Description("Creates a message payload that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed0PulseTimePulseCounterPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led0PulseTimePulseCounter { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0PulseTimePulseCounter register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led0PulseTimePulseCounter;
        }

        /// <summary>
        /// Creates a message that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PulseTimePulseCounter register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimePulseCounter.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed0PulseTimePulseCounterPayload")]
    [Description("Creates a timestamped message payload that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed0PulseTimePulseCounterPayload : CreateLed0PulseTimePulseCounterPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the number of pulses of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PulseTimePulseCounter register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimePulseCounter.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led0PulseTimeTailPayload")]
    [Description("Creates a message payload that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed0PulseTimeTailPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led0PulseTimeTail { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0PulseTimeTail register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led0PulseTimeTail;
        }

        /// <summary>
        /// Creates a message that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PulseTimeTail register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimeTail.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed0PulseTimeTailPayload")]
    [Description("Creates a timestamped message payload that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed0PulseTimeTailPayload : CreateLed0PulseTimeTailPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the wait time between pulses (milliseconds) of LED0 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PulseTimeTail register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PulseTimeTail.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led0PulseRepeatCounterPayload")]
    [Description("Creates a message payload that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed0PulseRepeatCounterPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.")]
        public ushort Led0PulseRepeatCounter { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led0PulseRepeatCounter register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led0PulseRepeatCounter;
        }

        /// <summary>
        /// Creates a message that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PulseRepeatCounter register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PulseRepeatCounter.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed0PulseRepeatCounterPayload")]
    [Description("Creates a timestamped message payload that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed0PulseRepeatCounterPayload : CreateLed0PulseRepeatCounterPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the number of repetitions of LED0 pulse protocol when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PulseRepeatCounter register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PulseRepeatCounter.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.
    /// </summary>
    [DisplayName("Led1PwmFrequencyPayload")]
    [Description("Creates a message payload that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.")]
    public partial class CreateLed1PwmFrequencyPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.
        /// </summary>
        [Range(min: 0.5, max: 2000)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.")]
        public float Led1PwmFrequency { get; set; } = 0.5F;

        /// <summary>
        /// Creates a message payload for the Led1PwmFrequency register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led1PwmFrequency;
        }

        /// <summary>
        /// Creates a message that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PwmFrequency register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PwmFrequency.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.
    /// </summary>
    [DisplayName("TimestampedLed1PwmFrequencyPayload")]
    [Description("Creates a timestamped message payload that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.")]
    public partial class CreateTimestampedLed1PwmFrequencyPayload : CreateLed1PwmFrequencyPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the frequency (Hz) of LED1 when in Pwm mode, between 0.5 and 2000.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PwmFrequency register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PwmFrequency.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.
    /// </summary>
    [DisplayName("Led1PwmDutyCyclePayload")]
    [Description("Creates a message payload that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.")]
    public partial class CreateLed1PwmDutyCyclePayload
    {
        /// <summary>
        /// Gets or sets the value that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.
        /// </summary>
        [Range(min: 0.1, max: 99.9)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.")]
        public float Led1PwmDutyCycle { get; set; } = 0.1F;

        /// <summary>
        /// Creates a message payload for the Led1PwmDutyCycle register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led1PwmDutyCycle;
        }

        /// <summary>
        /// Creates a message that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PwmDutyCycle register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PwmDutyCycle.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.
    /// </summary>
    [DisplayName("TimestampedLed1PwmDutyCyclePayload")]
    [Description("Creates a timestamped message payload that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.")]
    public partial class CreateTimestampedLed1PwmDutyCyclePayload : CreateLed1PwmDutyCyclePayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the duty cycle (%) of LED1 when in Pwm mode, between 0.1 and 99.9.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PwmDutyCycle register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PwmDutyCycle.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led1PwmPulseCounterPayload")]
    [Description("Creates a message payload that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.")]
    public partial class CreateLed1PwmPulseCounterPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.")]
        public ushort Led1PwmPulseCounter { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1PwmPulseCounter register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led1PwmPulseCounter;
        }

        /// <summary>
        /// Creates a message that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PwmPulseCounter register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PwmPulseCounter.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed1PwmPulseCounterPayload")]
    [Description("Creates a timestamped message payload that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed1PwmPulseCounterPayload : CreateLed1PwmPulseCounterPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the number of pulses of LED1 when in Pwm mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PwmPulseCounter register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PwmPulseCounter.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led1PulseTimeOnPayload")]
    [Description("Creates a message payload that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed1PulseTimeOnPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led1PulseTimeOn { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1PulseTimeOn register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led1PulseTimeOn;
        }

        /// <summary>
        /// Creates a message that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PulseTimeOn register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimeOn.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed1PulseTimeOnPayload")]
    [Description("Creates a timestamped message payload that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed1PulseTimeOnPayload : CreateLed1PulseTimeOnPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the time on (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PulseTimeOn register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimeOn.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led1PulseTimeOffPayload")]
    [Description("Creates a message payload that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed1PulseTimeOffPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led1PulseTimeOff { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1PulseTimeOff register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led1PulseTimeOff;
        }

        /// <summary>
        /// Creates a message that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PulseTimeOff register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimeOff.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed1PulseTimeOffPayload")]
    [Description("Creates a timestamped message payload that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed1PulseTimeOffPayload : CreateLed1PulseTimeOffPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the time off (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PulseTimeOff register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimeOff.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led1PulseTimePulseCounterPayload")]
    [Description("Creates a message payload that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed1PulseTimePulseCounterPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led1PulseTimePulseCounter { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1PulseTimePulseCounter register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led1PulseTimePulseCounter;
        }

        /// <summary>
        /// Creates a message that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PulseTimePulseCounter register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimePulseCounter.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed1PulseTimePulseCounterPayload")]
    [Description("Creates a timestamped message payload that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed1PulseTimePulseCounterPayload : CreateLed1PulseTimePulseCounterPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the number of pulses of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PulseTimePulseCounter register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimePulseCounter.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led1PulseTimeTailPayload")]
    [Description("Creates a message payload that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed1PulseTimeTailPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
        public ushort Led1PulseTimeTail { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1PulseTimeTail register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led1PulseTimeTail;
        }

        /// <summary>
        /// Creates a message that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PulseTimeTail register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimeTail.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed1PulseTimeTailPayload")]
    [Description("Creates a timestamped message payload that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed1PulseTimeTailPayload : CreateLed1PulseTimeTailPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the wait time between pulses (milliseconds) of LED1 when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PulseTimeTail register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PulseTimeTail.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("Led1PulseRepeatCounterPayload")]
    [Description("Creates a message payload that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateLed1PulseRepeatCounterPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.
        /// </summary>
        [Range(min: 1, max: 65535)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.")]
        public ushort Led1PulseRepeatCounter { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the Led1PulseRepeatCounter register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public ushort GetPayload()
        {
            return Led1PulseRepeatCounter;
        }

        /// <summary>
        /// Creates a message that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PulseRepeatCounter register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PulseRepeatCounter.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.
    /// </summary>
    [DisplayName("TimestampedLed1PulseRepeatCounterPayload")]
    [Description("Creates a timestamped message payload that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.")]
    public partial class CreateTimestampedLed1PulseRepeatCounterPayload : CreateLed1PulseRepeatCounterPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the number of repetitions of LED1 pulse protocol when in PulseTime mode, between 1 and 65535.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PulseRepeatCounter register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PulseRepeatCounter.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that get the real frequency (Hz) of LED0 when in Pwm mode.
    /// </summary>
    [DisplayName("Led0PwmRealPayload")]
    [Description("Creates a message payload that get the real frequency (Hz) of LED0 when in Pwm mode.")]
    public partial class CreateLed0PwmRealPayload
    {
        /// <summary>
        /// Gets or sets the value that get the real frequency (Hz) of LED0 when in Pwm mode.
        /// </summary>
        [Description("The value that get the real frequency (Hz) of LED0 when in Pwm mode.")]
        public float Led0PwmReal { get; set; }

        /// <summary>
        /// Creates a message payload for the Led0PwmReal register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led0PwmReal;
        }

        /// <summary>
        /// Creates a message that get the real frequency (Hz) of LED0 when in Pwm mode.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PwmReal register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PwmReal.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that get the real frequency (Hz) of LED0 when in Pwm mode.
    /// </summary>
    [DisplayName("TimestampedLed0PwmRealPayload")]
    [Description("Creates a timestamped message payload that get the real frequency (Hz) of LED0 when in Pwm mode.")]
    public partial class CreateTimestampedLed0PwmRealPayload : CreateLed0PwmRealPayload
    {
        /// <summary>
        /// Creates a timestamped message that get the real frequency (Hz) of LED0 when in Pwm mode.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PwmReal register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PwmReal.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that get the real duty cycle (%) of LED0 when in Pwm mode.
    /// </summary>
    [DisplayName("Led0PwmDutyCycleRealPayload")]
    [Description("Creates a message payload that get the real duty cycle (%) of LED0 when in Pwm mode.")]
    public partial class CreateLed0PwmDutyCycleRealPayload
    {
        /// <summary>
        /// Gets or sets the value that get the real duty cycle (%) of LED0 when in Pwm mode.
        /// </summary>
        [Description("The value that get the real duty cycle (%) of LED0 when in Pwm mode.")]
        public float Led0PwmDutyCycleReal { get; set; }

        /// <summary>
        /// Creates a message payload for the Led0PwmDutyCycleReal register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led0PwmDutyCycleReal;
        }

        /// <summary>
        /// Creates a message that get the real duty cycle (%) of LED0 when in Pwm mode.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led0PwmDutyCycleReal register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led0PwmDutyCycleReal.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that get the real duty cycle (%) of LED0 when in Pwm mode.
    /// </summary>
    [DisplayName("TimestampedLed0PwmDutyCycleRealPayload")]
    [Description("Creates a timestamped message payload that get the real duty cycle (%) of LED0 when in Pwm mode.")]
    public partial class CreateTimestampedLed0PwmDutyCycleRealPayload : CreateLed0PwmDutyCycleRealPayload
    {
        /// <summary>
        /// Creates a timestamped message that get the real duty cycle (%) of LED0 when in Pwm mode.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led0PwmDutyCycleReal register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led0PwmDutyCycleReal.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that get the real frequency (Hz) of LED1 when in Pwm mode.
    /// </summary>
    [DisplayName("Led1PwmRealPayload")]
    [Description("Creates a message payload that get the real frequency (Hz) of LED1 when in Pwm mode.")]
    public partial class CreateLed1PwmRealPayload
    {
        /// <summary>
        /// Gets or sets the value that get the real frequency (Hz) of LED1 when in Pwm mode.
        /// </summary>
        [Description("The value that get the real frequency (Hz) of LED1 when in Pwm mode.")]
        public float Led1PwmReal { get; set; }

        /// <summary>
        /// Creates a message payload for the Led1PwmReal register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led1PwmReal;
        }

        /// <summary>
        /// Creates a message that get the real frequency (Hz) of LED1 when in Pwm mode.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PwmReal register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PwmReal.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that get the real frequency (Hz) of LED1 when in Pwm mode.
    /// </summary>
    [DisplayName("TimestampedLed1PwmRealPayload")]
    [Description("Creates a timestamped message payload that get the real frequency (Hz) of LED1 when in Pwm mode.")]
    public partial class CreateTimestampedLed1PwmRealPayload : CreateLed1PwmRealPayload
    {
        /// <summary>
        /// Creates a timestamped message that get the real frequency (Hz) of LED1 when in Pwm mode.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PwmReal register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PwmReal.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that get the real duty cycle (%) of LED1 when in Pwm mode.
    /// </summary>
    [DisplayName("Led1PwmDutyCycleRealPayload")]
    [Description("Creates a message payload that get the real duty cycle (%) of LED1 when in Pwm mode.")]
    public partial class CreateLed1PwmDutyCycleRealPayload
    {
        /// <summary>
        /// Gets or sets the value that get the real duty cycle (%) of LED1 when in Pwm mode.
        /// </summary>
        [Description("The value that get the real duty cycle (%) of LED1 when in Pwm mode.")]
        public float Led1PwmDutyCycleReal { get; set; }

        /// <summary>
        /// Creates a message payload for the Led1PwmDutyCycleReal register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public float GetPayload()
        {
            return Led1PwmDutyCycleReal;
        }

        /// <summary>
        /// Creates a message that get the real duty cycle (%) of LED1 when in Pwm mode.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the Led1PwmDutyCycleReal register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.Led1PwmDutyCycleReal.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that get the real duty cycle (%) of LED1 when in Pwm mode.
    /// </summary>
    [DisplayName("TimestampedLed1PwmDutyCycleRealPayload")]
    [Description("Creates a timestamped message payload that get the real duty cycle (%) of LED1 when in Pwm mode.")]
    public partial class CreateTimestampedLed1PwmDutyCycleRealPayload : CreateLed1PwmDutyCycleRealPayload
    {
        /// <summary>
        /// Creates a timestamped message that get the real duty cycle (%) of LED1 when in Pwm mode.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the Led1PwmDutyCycleReal register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.Led1PwmDutyCycleReal.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that write the state of the auxiliary digital output bit.
    /// </summary>
    [DisplayName("AuxDigitalOutputStatePayload")]
    [Description("Creates a message payload that write the state of the auxiliary digital output bit.")]
    public partial class CreateAuxDigitalOutputStatePayload
    {
        /// <summary>
        /// Gets or sets the value that write the state of the auxiliary digital output bit.
        /// </summary>
        [Description("The value that write the state of the auxiliary digital output bit.")]
        public AuxDigitalOutputs AuxDigitalOutputState { get; set; }

        /// <summary>
        /// Creates a message payload for the AuxDigitalOutputState register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public AuxDigitalOutputs GetPayload()
        {
            return AuxDigitalOutputState;
        }

        /// <summary>
        /// Creates a message that write the state of the auxiliary digital output bit.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the AuxDigitalOutputState register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.AuxDigitalOutputState.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that write the state of the auxiliary digital output bit.
    /// </summary>
    [DisplayName("TimestampedAuxDigitalOutputStatePayload")]
    [Description("Creates a timestamped message payload that write the state of the auxiliary digital output bit.")]
    public partial class CreateTimestampedAuxDigitalOutputStatePayload : CreateAuxDigitalOutputStatePayload
    {
        /// <summary>
        /// Creates a timestamped message that write the state of the auxiliary digital output bit.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the AuxDigitalOutputState register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.AuxDigitalOutputState.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that sets the power to be applied to auxiliary LED, between 1 and 120.
    /// </summary>
    [DisplayName("AuxLedPowerPayload")]
    [Description("Creates a message payload that sets the power to be applied to auxiliary LED, between 1 and 120.")]
    public partial class CreateAuxLedPowerPayload
    {
        /// <summary>
        /// Gets or sets the value that sets the power to be applied to auxiliary LED, between 1 and 120.
        /// </summary>
        [Range(min: 1, max: 120)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The value that sets the power to be applied to auxiliary LED, between 1 and 120.")]
        public byte AuxLedPower { get; set; } = 1;

        /// <summary>
        /// Creates a message payload for the AuxLedPower register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public byte GetPayload()
        {
            return AuxLedPower;
        }

        /// <summary>
        /// Creates a message that sets the power to be applied to auxiliary LED, between 1 and 120.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the AuxLedPower register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.AuxLedPower.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that sets the power to be applied to auxiliary LED, between 1 and 120.
    /// </summary>
    [DisplayName("TimestampedAuxLedPowerPayload")]
    [Description("Creates a timestamped message payload that sets the power to be applied to auxiliary LED, between 1 and 120.")]
    public partial class CreateTimestampedAuxLedPowerPayload : CreateAuxLedPowerPayload
    {
        /// <summary>
        /// Creates a timestamped message that sets the power to be applied to auxiliary LED, between 1 and 120.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the AuxLedPower register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.AuxLedPower.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that write the state of digital output lines.
    /// </summary>
    [DisplayName("DigitalOutputStatePayload")]
    [Description("Creates a message payload that write the state of digital output lines.")]
    public partial class CreateDigitalOutputStatePayload
    {
        /// <summary>
        /// Gets or sets the value that write the state of digital output lines.
        /// </summary>
        [Description("The value that write the state of digital output lines.")]
        public DigitalOutputs DigitalOutputState { get; set; }

        /// <summary>
        /// Creates a message payload for the DigitalOutputState register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public DigitalOutputs GetPayload()
        {
            return DigitalOutputState;
        }

        /// <summary>
        /// Creates a message that write the state of digital output lines.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the DigitalOutputState register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.DigitalOutputState.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that write the state of digital output lines.
    /// </summary>
    [DisplayName("TimestampedDigitalOutputStatePayload")]
    [Description("Creates a timestamped message payload that write the state of digital output lines.")]
    public partial class CreateTimestampedDigitalOutputStatePayload : CreateDigitalOutputStatePayload
    {
        /// <summary>
        /// Creates a timestamped message that write the state of digital output lines.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the DigitalOutputState register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.DigitalOutputState.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a message payload
    /// that specifies all the active events in the device.
    /// </summary>
    [DisplayName("EnableEventsPayload")]
    [Description("Creates a message payload that specifies all the active events in the device.")]
    public partial class CreateEnableEventsPayload
    {
        /// <summary>
        /// Gets or sets the value that specifies all the active events in the device.
        /// </summary>
        [Description("The value that specifies all the active events in the device.")]
        public LedArrayEvents EnableEvents { get; set; }

        /// <summary>
        /// Creates a message payload for the EnableEvents register.
        /// </summary>
        /// <returns>The created message payload value.</returns>
        public LedArrayEvents GetPayload()
        {
            return EnableEvents;
        }

        /// <summary>
        /// Creates a message that specifies all the active events in the device.
        /// </summary>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new message for the EnableEvents register.</returns>
        public HarpMessage GetMessage(MessageType messageType)
        {
            return Harp.LedArray.EnableEvents.FromPayload(messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents an operator that creates a timestamped message payload
    /// that specifies all the active events in the device.
    /// </summary>
    [DisplayName("TimestampedEnableEventsPayload")]
    [Description("Creates a timestamped message payload that specifies all the active events in the device.")]
    public partial class CreateTimestampedEnableEventsPayload : CreateEnableEventsPayload
    {
        /// <summary>
        /// Creates a timestamped message that specifies all the active events in the device.
        /// </summary>
        /// <param name="timestamp">The timestamp of the message payload, in seconds.</param>
        /// <param name="messageType">Specifies the type of the created message.</param>
        /// <returns>A new timestamped message for the EnableEvents register.</returns>
        public HarpMessage GetMessage(double timestamp, MessageType messageType)
        {
            return Harp.LedArray.EnableEvents.FromPayload(timestamp, messageType, GetPayload());
        }
    }

    /// <summary>
    /// Represents the payload of the DigitalOutputSync register.
    /// </summary>
    public struct DigitalOutputSyncPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalOutputSyncPayload"/> structure.
        /// </summary>
        /// <param name="dO0Sync">Configuration of the DO0 functionality.</param>
        /// <param name="dO1Sync">Configuration of the DO1 functionality.</param>
        public DigitalOutputSyncPayload(
            DO0SyncConfig dO0Sync,
            DO1SyncConfig dO1Sync)
        {
            DO0Sync = dO0Sync;
            DO1Sync = dO1Sync;
        }

        /// <summary>
        /// Configuration of the DO0 functionality.
        /// </summary>
        public DO0SyncConfig DO0Sync;

        /// <summary>
        /// Configuration of the DO1 functionality.
        /// </summary>
        public DO1SyncConfig DO1Sync;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the payload of
        /// the DigitalOutputSync register.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the payload of the
        /// DigitalOutputSync register.
        /// </returns>
        public override string ToString()
        {
            return "DigitalOutputSyncPayload { " +
                "DO0Sync = " + DO0Sync + ", " +
                "DO1Sync = " + DO1Sync + " " +
            "}";
        }
    }

    /// <summary>
    /// Represents the payload of the DigitalInputTrigger register.
    /// </summary>
    public struct DigitalInputTriggerPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalInputTriggerPayload"/> structure.
        /// </summary>
        /// <param name="dI0Trigger">Configuration of the DIO input pin.</param>
        /// <param name="dI1Trigger">Configuration of the DI1 input pin.</param>
        public DigitalInputTriggerPayload(
            DigitalInputTriggerConfig dI0Trigger,
            DigitalInputTriggerConfig dI1Trigger)
        {
            DI0Trigger = dI0Trigger;
            DI1Trigger = dI1Trigger;
        }

        /// <summary>
        /// Configuration of the DIO input pin.
        /// </summary>
        public DigitalInputTriggerConfig DI0Trigger;

        /// <summary>
        /// Configuration of the DI1 input pin.
        /// </summary>
        public DigitalInputTriggerConfig DI1Trigger;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the payload of
        /// the DigitalInputTrigger register.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the payload of the
        /// DigitalInputTrigger register.
        /// </returns>
        public override string ToString()
        {
            return "DigitalInputTriggerPayload { " +
                "DI0Trigger = " + DI0Trigger + ", " +
                "DI1Trigger = " + DI1Trigger + " " +
            "}";
        }
    }

    /// <summary>
    /// Represents the payload of the PulseMode register.
    /// </summary>
    public struct PulseModePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PulseModePayload"/> structure.
        /// </summary>
        /// <param name="led0Mode">Sets the pulse mode used in LED0.</param>
        /// <param name="led1Mode">Sets the pulse mode used in LED0</param>
        public PulseModePayload(
            PulseModeConfig led0Mode,
            PulseModeConfig led1Mode)
        {
            Led0Mode = led0Mode;
            Led1Mode = led1Mode;
        }

        /// <summary>
        /// Sets the pulse mode used in LED0.
        /// </summary>
        public PulseModeConfig Led0Mode;

        /// <summary>
        /// Sets the pulse mode used in LED0
        /// </summary>
        public PulseModeConfig Led1Mode;

        /// <summary>
        /// Returns a <see cref="string"/> that represents the payload of
        /// the PulseMode register.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the payload of the
        /// PulseMode register.
        /// </returns>
        public override string ToString()
        {
            return "PulseModePayload { " +
                "Led0Mode = " + Led0Mode + ", " +
                "Led1Mode = " + Led1Mode + " " +
            "}";
        }
    }

    /// <summary>
    /// Specifies the LEDs state.
    /// </summary>
    [Flags]
    public enum LedState : byte
    {
        None = 0x0,
        Led0On = 0x1,
        Led1On = 0x2,
        Led0Off = 0x4,
        Led1Off = 0x8
    }

    /// <summary>
    /// Specifies the state of port digital input lines.
    /// </summary>
    [Flags]
    public enum DigitalInputs : byte
    {
        None = 0x0,
        DI0 = 0x1,
        DI1 = 0x2
    }

    /// <summary>
    /// Specifies the state of the auxiliary digital output lines.
    /// </summary>
    [Flags]
    public enum AuxDigitalOutputs : byte
    {
        None = 0x0,
        Aux0Set = 0x1,
        Aux1Set = 0x2,
        Aux0Clear = 0x4,
        Aux1Clear = 0x8
    }

    /// <summary>
    /// Specifies the state of port digital output lines.
    /// </summary>
    [Flags]
    public enum DigitalOutputs : byte
    {
        None = 0x0,
        DO0Set = 0x1,
        DO1Set = 0x2,
        DO0Clear = 0x4,
        DO1Clear = 0x8
    }

    /// <summary>
    /// The events that can be enabled/disabled.
    /// </summary>
    [Flags]
    public enum LedArrayEvents : byte
    {
        None = 0x0,
        EnableLed = 0x1,
        DigitalInputState = 0x2
    }

    /// <summary>
    /// Available configurations when using digital output pin 0 to report firmware events.
    /// </summary>
    public enum DO0SyncConfig : byte
    {
        None = 0,
        MimicLed0EnablePower = 1,
        MimicLed0EnableBehavior = 2,
        MimicLed0EnableLed = 3
    }

    /// <summary>
    /// Available configurations when using digital output pin 1 to report firmware events.
    /// </summary>
    public enum DO1SyncConfig : byte
    {
        None = 0,
        MimicLed1EnablePower = 16,
        MimicLed1EnableBehavior = 32,
        MimicLed1EnableLed = 48
    }

    /// <summary>
    /// Available configurations when using digital inputs as an acquisition trigger.
    /// </summary>
    public enum DigitalInputTriggerConfig : byte
    {
        Led0EnablePower = 0,
        Led0EnableBehavior = 1,
        Led0EnableLed = 2,
        Led1EnablePower = 3,
        Led1EnableBehavior = 4,
        Led1EnableLed = 5,
        None = 6
    }

    /// <summary>
    /// Available configurations modes when LED behavior is enabled.
    /// </summary>
    public enum PulseModeConfig : byte
    {
        Pwm = 0,
        PulseTime = 1
    }
}
