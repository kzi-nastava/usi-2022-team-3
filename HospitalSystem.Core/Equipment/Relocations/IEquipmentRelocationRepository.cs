namespace HospitalSystem.Core.Equipment;

public interface IEquipmentRelocationRepository
{
    public IQueryable<EquipmentRelocation> GetAll();
    
    public void Insert(EquipmentRelocation relocation);

    public void Replace(EquipmentRelocation relocation); // NOTE: expects existing!!
}