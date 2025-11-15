using CsvHelper.Configuration;

namespace CsvParser.Models;

public class CabDataCsvMap : ClassMap<CabData>
{
    public CabDataCsvMap()
    {
        Map(m => m.TpepPickupDatetime).Name("tpep_pickup_datetime").Optional();
        Map(m => m.TpepDropoffDatetime).Name("tpep_dropoff_datetime").Optional();
        Map(m => m.PassengerCount).Name("passenger_count").Optional();
        Map(m => m.TripDistance).Name("trip_distance").Optional();
        Map(m => m.StoreAndFwdFlag).Name("store_and_fwd_flag").Optional();
        Map(m => m.PULocationID).Name("PULocationID").Optional();
        Map(m => m.DOLocationID).Name("DOLocationID").Optional();
        Map(m => m.FareAmount).Name("fare_amount").Optional();
        Map(m => m.TipAmount).Name("tip_amount").Optional();
    }
}
