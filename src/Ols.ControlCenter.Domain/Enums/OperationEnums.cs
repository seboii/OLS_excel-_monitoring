namespace Ols.ControlCenter.Domain.Enums;

/// <summary>Ana taşıma tipi — operasyonun hangi departman ekranında görüneceğini belirler.</summary>
public enum TransportType
{
    Road,
    Sea,
    Air,
    Customs,
    Other
}

/// <summary>Hizmet/yük tipi.</summary>
public enum ServiceType
{
    Ltl,
    Ftl,
    Fcl,
    Lcl,
    AirCargo,
    Customs,
    Parcel,
    Other
}

/// <summary>İthalat / ihracat / transit yönü.</summary>
public enum TradeDirection
{
    None,
    Import,
    Export,
    Transit
}

/// <summary>Standartlaştırılmış operasyon statüsü (kaynak statüleri buna normalize edilir).</summary>
public enum OperationStatus
{
    New,
    Preparing,
    AwaitingLoading,
    InTransit,
    AtPort,
    InCustoms,
    InWarehouse,
    OutForDelivery,
    Completed,
    OnHold,
    Delayed,
    FinancialHold,
    MissingDocuments,
    Cancelled
}

/// <summary>
/// Risk seviyeleri. Sayısal sıra ANLAMLIDIR: risk motoru birden çok kuraldan gelen
/// en yüksek (max) seviyeyi operasyona yazar. Green &lt; Yellow &lt; Orange &lt; Red &lt; Black.
/// </summary>
public enum RiskLevel
{
    Green = 0,
    Yellow = 1,
    Orange = 2,
    Red = 3,
    Black = 4
}

/// <summary>Gecikme sebebi sınıflandırması.</summary>
public enum DelayReason
{
    None,
    VehicleLate,
    LoadingDelay,
    MissingDocuments,
    CustomsHold,
    BorderCongestion,
    CustomerCaused,
    CarrierCaused,
    FinancialApprovalPending,
    Other
}
