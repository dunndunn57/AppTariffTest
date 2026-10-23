using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RecordApp.Models
{
    [XmlRoot("Tariff")]
    public class Tariff : INotifyPropertyChanged
    {
        // 1. These can stay as modern auto-properties if they NEVER change after creation


        [XmlElement("Id")]
        // public Guid Id { get; init; } // AI (20/09/2026) 2. Consider replacing the init accessors 
        // with ordinary set accessors if the class is intended to be an XML-persisted data object. 
        public Guid Id { get; set; }

        [XmlElement("UnitPrice")]
        // public decimal UnitPrice { get; init; }
        public decimal UnitPrice { get; set; }

        // Only USE THE FOLLOWING if actual tests reveal a serialization issue.

        [XmlIgnore]
        public DateOnly EffectiveFrom { get; set; }

        [XmlElement("EffectiveFrom")]
        public string EffectiveFromXml
        {
            get => EffectiveFrom.ToString("yyyy-MM-dd");
            set => EffectiveFrom = DateOnly.Parse(value);
        }

        /* 21 August 2026
        [XmlElement("EffectiveFrom")]
        // public DateOnly EffectiveFrom { get; init; }
        public DateOnly EffectiveFrom { get; set; }
        */

        // 2. These CAN change, so they need explicit backing fields to alert WPF
        private DateOnly? _effectiveTo;

        [XmlElement("EffectiveTo")]
        public DateOnly? EffectiveTo
        {
            get => _effectiveTo;
            set
            {
                if (_effectiveTo != value)
                {
                    _effectiveTo = value;
                    OnPropertyChanged();
                }
            }
        }


        private TariffStatus _status;


        /*
        [XmlElement("Status")]
        public TariffStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;

                // Enforce FSM Transition Guard Rules
                if (!IsValidTransition(_status, value))
                {
                    throw new InvalidOperationException($"Invalid tariff state transition from {_status} to {value}.");
                }

                _status = value;
                OnPropertyChanged();
            }
        }
        */

        [XmlElement("Status")]
        public TariffStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public void Publish()
        {
            this.SetStatus(TariffStatus.Published);
        }

        public void Suspend()
        {
            this.SetStatus(TariffStatus.Suspended);
        }

        public void Supersede()
        {
            this.SetStatus(TariffStatus.Superseded);
        }
        /*
        private void SetStatus(TariffStatus newStatus)
        {
            if (_status == newStatus) return;
            // Enforce FSM Transition Guard Rules
            if (!IsValidTransition(_status, newStatus))
            {
                throw new InvalidOperationException($"Invalid tariff state transition from {_status} to {newStatus}.");
            }
            _status = newStatus;
            OnPropertyChanged();
        }
        */

        private void SetStatus(TariffStatus newStatus)
        {
            if (_status == newStatus) return;
            // Enforce FSM Transition Guard Rules
            if (!IsValidTransition(_status, newStatus))
            {
                throw new InvalidOperationException($"Invalid tariff state transition from {_status} to {newStatus}.");
            }
            _status = newStatus;
            // OnPropertyChanged(); AIMING FOR UI CORRECT UPDATE (ROUGHLY)
            OnPropertyChanged(nameof(Status));
        }

        // Helper method defining the FSM rules
        private static bool IsValidTransition(TariffStatus current, TariffStatus next)
        {
            return current switch
            {
                TariffStatus.Draft => next == TariffStatus.Published,

                TariffStatus.Published => next == TariffStatus.Suspended ||
                                                next == TariffStatus.Superseded,

                TariffStatus.Suspended => next == TariffStatus.Published ||
                                                next == TariffStatus.Superseded,

                TariffStatus.Superseded => false, // Terminal state

                _ => false

            };
        }


        public string DisplayName => $"{UnitPrice:C} ({EffectiveFrom:d} - {EffectiveTo:d})";


        // 3. Your constructor remains clean and functional

        public Tariff()
        {
        }

        /*
        public Tariff() : this(Guid.Empty, 0m, default, null, default)
        {
        }
        */

        public Tariff(Guid id, decimal unitPrice, DateOnly effectiveFrom, DateOnly? effectiveTo, TariffStatus status)
        {
            Id = id;
            UnitPrice = unitPrice;
            EffectiveFrom = effectiveFrom;
            EffectiveTo = effectiveTo;
            // Status = status;
            _status = status; // Direct assignment to avoid triggering FSM validation during construction
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}