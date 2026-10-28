' Option Strict Off solo qui: late binding sul COM NetworkListManager evita di dichiarare tutte le sue interfacce.
Option Strict Off

Imports System.Net.NetworkInformation
Imports System.Net.Sockets

Public Module Rete
    ''' <summary>Nomi profili di rete connessi, suffissi DNS e gateway IPv4 delle schede attive.</summary>
    Public Function Identificativi() As List(Of String)
        Dim ris As New List(Of String)(NomiProfili())
        For Each nic In NetworkInterface.GetAllNetworkInterfaces()
            If nic.OperationalStatus <> OperationalStatus.Up OrElse nic.NetworkInterfaceType = NetworkInterfaceType.Loopback Then Continue For
            Dim p = nic.GetIPProperties()
            If p.DnsSuffix <> "" Then ris.Add(p.DnsSuffix)
            For Each g In p.GatewayAddresses
                If g.Address.AddressFamily = AddressFamily.InterNetwork AndAlso g.Address.ToString() <> "0.0.0.0" Then ris.Add(g.Address.ToString())
            Next
        Next
        Return ris.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
    End Function

    ''' <summary>Nomi delle reti connesse come li mostra Windows (per il Wi-Fi di solito è l'SSID).</summary>
    Public Function NomiProfili() As List(Of String)
        Dim nomi As New List(Of String)
        Dim nlm As Object = Activator.CreateInstance(Type.GetTypeFromCLSID(New Guid("DCB00C01-570F-4A9B-8D69-199FDBA5723B")))
        For Each rete As Object In nlm.GetNetworks(1) ' NLM_ENUM_NETWORK_CONNECTED
            nomi.Add(CStr(rete.GetName()))
        Next
        Return nomi
    End Function
End Module
