namespace SkyHubTycoon.Data
{
    public enum ZoneType
    {
        Entrance,
        Security,
        Waiting,
        Gate,
        Baggage,
        Shop,
        Bathroom,
        Staff,
        VIP,
        Customs,
        Airfield
    }

    public enum BuildCategory
    {
        Floors,
        WallsAndDoors,
        PassengerProcessing,
        GatesAndFlights,
        Airfield,
        Baggage,
        Comfort,
        Shops,
        Staff,
        Utilities
    }

    public enum BuildableType
    {
        Entrance,
        CheckIn,
        Kiosk,
        Security,
        Seating,
        SmallGate,
        Runway,
        Taxiway,
        BagDrop,
        Conveyor,
        Carousel,
        Bathroom,
        Coffee,
        StaffRoom,
        Generator,
        WaterHub,
        PassportControl
    }

    public enum AgentPathType
    {
        Passenger,
        Staff,
        Baggage,
        Vehicle
    }
}
