Imports DataProtector
Imports System.Text
Imports System.Configuration

Module ConexionBD

    Dim dp As New DataProtector.DataProtector(DataProtector.DataProtector.Store.USE_MACHINE_STORE)
    'Dim appSettingValue As String = System.Configuration.ConfigurationSettings.AppSettings("connectionStringCuentas1") 'PARA PROD
    'Dim appSettingValue As String = System.Configuration.ConfigurationSettings.AppSettings("connectionStringCuentas3") 'PARA DESO

    'Dim dataToDecrypt As Byte() = Convert.FromBase64String(appSettingValue)
    'Public conexion As String = Encoding.ASCII.GetString(dp.Decrypt(dataToDecrypt, Nothing))

    'Dim appSettingValue1 As String = System.Configuration.ConfigurationSettings.AppSettings("connectionStringCuentas2") 'PARA PROD
    'Dim appSettingValue1 As String = System.Configuration.ConfigurationSettings.AppSettings("connectionStringCuentas4")  'PARA DESO
    'Dim dataToDecrypt1 As Byte() = Convert.FromBase64String(appSettingValue1)
    'Public conexion2 As String = Encoding.ASCII.GetString(dp.Decrypt(dataToDecrypt1, Nothing))

    'Dim appSettingValue2 As String = System.Configuration.ConfigurationSettings.AppSettings("ConnectionSPRING")
    'Dim dataToDecrypt2 As Byte() = Convert.FromBase64String(appSettingValue2)
    'Public conexionSQLServer As String = Encoding.ASCII.GetString(dp.Decrypt(dataToDecrypt2, Nothing))

    '' comentar antes de pasar a DESO **********************************************
    'Temporal DESI  -- PROD
    Public conexion As String = "Server=SVRDESE2;Data source=DESE2;User id=master;password=s1mps0n"

    'Temporal DESE -- PRODSI2
    Public conexion2 As String = "Server=DESE2;Data source=DESE2;User id=master;password=s1mps0n"

    Public conexionSQLServer As String = "server=simbad3;uid=desarrollo;pwd=desasimbad;database=UPC_DES;Network=dbmssocn;"
    ' comentar antes de pasar a DESO **********************************************
End Module
