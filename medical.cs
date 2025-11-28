
using System;
using System.Collections.Generic;
using System.Linq;


namespace MedicalSystem
{
    public abstract class Person
    {
        public int Id { get; }
        public string Name { get; protected set; }
        public DateTime DateOfBirth { get; protected set; }
        public string Address { get; protected set; }

        protected Person(int id, string name, DateTime dateOfBirth, string address)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DateOfBirth = dateOfBirth;
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }
    }

    public class Patient : Person
    {
        public string MedicalHistorySummary { get; private set; }

        public Patient(int id, string name, DateTime dob, string address, string history)
            : base(id, name, dob, address)
        {
            MedicalHistorySummary = history;
        }

        public Appointment ScheduleAppointment(Doctor doctor, DateTime dateTime)
        {
            return new Appointment(0, this, doctor, dateTime);
        }

        public void UpdateData(string address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public void ViewHistory()
        {
            Console.WriteLine(MedicalHistorySummary);
        }
    }

    public abstract class MedicalStaff : Person
    {
        public string Specialization { get; protected set; }
        public string Room { get; protected set; }
        public string WorkSchedule { get; protected set; }

        protected MedicalStaff(int id, string name, DateTime dob, string address,
            string specialization, string room, string workSchedule)
            : base(id, name, dob, address)
        {
            Specialization = specialization;
            Room = room;
            WorkSchedule = workSchedule;
        }
    }

    public class Doctor : MedicalStaff
    {
        public Doctor(int id, string name, DateTime dob, string address,
            string specialization, string room, string workSchedule)
            : base(id, name, dob, address, specialization, room, workSchedule)
        {
        }

        public void AssignTreatment(MedicalRecord record, string diagnosis)
        {
            var entry = new RecordEntry(
                id: record.GetNextEntryId(),
                visitDate: DateTime.Now,
                diagnosis: diagnosis,
                notes: "Назначено лечение",
                examinationResults: string.Empty);

            record.AddEntry(entry);
        }

        public void WritePrescription(Prescription prescription)
        {
            // Логика сохранения рецепта в системе
        }

        public void UpdateSchedule(string schedule)
        {
            WorkSchedule = schedule;
        }
    }

    public class Nurse : MedicalStaff
    {
        public Nurse(int id, string name, DateTime dob, string address,
            string specialization, string room, string workSchedule)
            : base(id, name, dob, address, specialization, room, workSchedule)
        {
        }

        public void AssistDoctor(Doctor doctor, Patient patient)
        {
            // Логика помощи врачу
        }
    }

    public enum AppointmentStatus
    {
        Scheduled,
        Cancelled,
        Completed
    }

    public class Appointment
    {
        public int Id { get; }
        public DateTime DateTime { get; private set; }
        public Patient Patient { get; }
        public Doctor Doctor { get; }
        public AppointmentStatus Status { get; private set; }

        public Appointment(int id, Patient patient, Doctor doctor, DateTime dateTime)
        {
            Id = id;
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));
            Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
            DateTime = dateTime;
            Status = AppointmentStatus.Scheduled;
        }

        public void Create()
        {
            Status = AppointmentStatus.Scheduled;
        }

        public void Cancel()
        {
            Status = AppointmentStatus.Cancelled;
        }

        public void Complete()
        {
            Status = AppointmentStatus.Completed;
        }
    }

    public class RecordEntry
    {
        public int Id { get; }
        public DateTime VisitDate { get; private set; }
        public string Diagnosis { get; private set; }
        public string Notes { get; private set; }
        public string ExaminationResults { get; private set; }

        public RecordEntry(int id, DateTime visitDate, string diagnosis, string notes, string examinationResults)
        {
            Id = id;
            VisitDate = visitDate;
            Diagnosis = diagnosis;
            Notes = notes;
            ExaminationResults = examinationResults;
        }

        public void UpdateDiagnosis(string diagnosis)
        {
            Diagnosis = diagnosis;
        }
    }

    public class MedicalRecord
    {
        public int Id { get; }
        public Patient Patient { get; }
        private readonly List<RecordEntry> _entries = new();

        public IReadOnlyCollection<RecordEntry> Entries => _entries.AsReadOnly();

        public MedicalRecord(int id, Patient patient)
        {
            Id = id;
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));
        }

        public void AddEntry(RecordEntry entry)
        {
            _entries.Add(entry);
        }

        public void UpdateDiagnosis(int entryId, string diagnosis)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == entryId);
            entry?.UpdateDiagnosis(diagnosis);
        }

        public void RemoveEntry(int entryId)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == entryId);
            if (entry != null)
            {
                _entries.Remove(entry);
            }
        }

        public int GetNextEntryId() => _entries.Count == 0 ? 1 : _entries.Max(e => e.Id) + 1;
    }

    public class Medication
    {
        public int Id { get; }
        public string Name { get; private set; }
        public int Quantity { get; private set; }
        public DateTime ExpirationDate { get; private set; }

        public Medication(int id, string name, int quantity, DateTime expirationDate)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Quantity = quantity;
            ExpirationDate = expirationDate;
        }

        public void Order(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Quantity += amount;
        }

        public void Dispense(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Quantity < amount) throw new InvalidOperationException("Not enough stock.");
            Quantity -= amount;
        }

        public void WriteOff(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Quantity = Math.Max(0, Quantity - amount);
        }
    }

    public class Prescription
    {
        public int Id { get; }
        public Doctor Doctor { get; }
        public Patient Patient { get; }
        public Medication Medication { get; }
        public string Dosage { get; private set; }
        public DateTime ValidUntil { get; private set; }
        public bool IsCancelled { get; private set; }

        public Prescription(int id, Doctor doctor, Patient patient,
            Medication medication, string dosage, DateTime validUntil)
        {
            Id = id;
            Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
            Patient = patient ?? throw new ArgumentNullException(nameof(patient));
            Medication = medication ?? throw new ArgumentNullException(nameof(medication));
            Dosage = dosage ?? throw new ArgumentNullException(nameof(dosage));
            ValidUntil = validUntil;
        }

        public void Create()
        {
            IsCancelled = false;
        }

        public void Update(string dosage, DateTime validUntil)
        {
            Dosage = dosage ?? throw new ArgumentNullException(nameof(dosage));
            ValidUntil = validUntil;
        }

        public void Cancel()
        {
            IsCancelled = true;
        }
    }

    public class PharmacyInventory
    {
        private readonly List<Medication> _medications = new();

        public IReadOnlyCollection<Medication> Medications => _medications.AsReadOnly();

        public void AddMedication(Medication medication)
        {
            _medications.Add(medication);
        }

        public Medication FindByName(string name)
        {
            return _medications.FirstOrDefault(m =>
                m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateStock(Medication medication, int delta)
        {
            if (!_medications.Contains(medication))
                throw new InvalidOperationException("Medication not in inventory.");

            if (delta > 0)
                medication.Order(delta);
            else if (delta < 0)
                medication.WriteOff(-delta);
        }
    }
}
