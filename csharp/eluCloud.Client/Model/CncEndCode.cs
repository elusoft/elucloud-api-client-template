namespace elusoft.eluCloud.Client.Model;

public enum CncEndCode
{
    ManualControllerReset = 1,
    EciReset = 2,
    ControlPanelReset = 3,
    AxisControllerError = 4,
    Ok = 5,
    SystemInit = 6,
    ErrorMessage = 7,
    CncError = 8,
    CncStop = 9,
    EmergencyStop = 10,
    UnknownReset = 99
}
