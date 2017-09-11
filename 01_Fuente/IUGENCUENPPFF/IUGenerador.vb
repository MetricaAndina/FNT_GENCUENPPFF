Module IUGenerador

    ' ***********************************************************
    ' Generador de cuentas de padres de familia
    ' Realizado por Renzo Bogovich 19/12/2010
    ' Para la SS-2012-056 (Administracion de cuentas padres de familia)
    ' ***********************************************************
    Sub Main()
        Dim iOK As Integer, iKO As Integer
        pLinea = System.Configuration.ConfigurationSettings.AppSettings("Linea")
        pModal = System.Configuration.ConfigurationSettings.AppSettings("Modalidad")
        pRuta = System.Configuration.ConfigurationSettings.AppSettings("rutaAD")

        from = System.Configuration.ConfigurationSettings.AppSettings("from")
        Destinatario1 = System.Configuration.ConfigurationSettings.AppSettings("Destinatario1")
        Destinatario2 = System.Configuration.ConfigurationSettings.AppSettings("Destinatario2")

        ' Creamos la conexion a la base de datos
        objLNGenerador = New LNGENADMPERS.LNGenerador(conexion, conexion2, conexionSQLServer)

        ' Obtenemos la fecha de inicio del proceso y el nombre sugerido del archivo
        iError = objLNGenerador.ParametrosInicioProceso(sFechaIniProceso, sNombreArchivoLOG, sMsgError)
        ManejaError(iError, sMsgError, True)

        ' Paso 1: Generamos un LOG de sucesos para el proceso 
        iError = utilCrearArchivo(sNombreArchivoLOG, sMsgError)
        ManejaError(iError, sMsgError, True)
        ' Grabamos la cabecera en el log
        GrabarCabeceraArchivo()

        ' Mostramos la cabecera en pantalla
        MostrarCabeceraPantalla()

        LogProceso("*******************************************************************")

        LogProceso("**********Proceso de identificación de padres de familia***********")
        LogProceso("Ciclo o Periodo: (Presionar enter si es el ciclo actual)")
        pPeriodo = Console.ReadLine() 'System.Configuration.ConfigurationSettings.AppSettings("Periodo")
        Dim x As String = objLNGenerador.SP_PROCESAR_CUENTA_ALUM(pLinea, pModal, pPeriodo, iOK)
        utilEscribir(pPeriodo, sMsgError)
        LogProceso("N° de alumnos registrados: " & iOK)
        LogProceso("*******************************************************************")

        ' Paso 2: Procesamos las cuentas de AD y Exchange
        iOK = 0
        iKO = 0
        sOutPut = ""
        LogProceso("**************Proceso de cuentas AD y buzones de Exchange**********")
        iError = objLNGenerador.ProcesarCuentasADExchange(pLinea, pPeriodo, pUnidadMapeo, iOK, iKO, sOutPut, sMsgError, pRuta)
        ManejaError(iError, sMsgError, False)
        LogProceso("Resultados: " & vbCrLf & sMsgError)
        LogProceso("*******************************************************************")
        LogProceso("N° de cuentas procesadas OK: " & iOK)
        LogProceso("N° de cuentas procesadas KO: " & iKO)
        LogProceso("*******************************************************************")


        ' Paso 4: Generamos excel con los procesados correctamente y enviamos el correo
        iOK = 0
        iKO = 0
        sOutPut = ""
        LogProceso("**************Generación del excel y envío del correo** ********")
        iError = objLNGenerador.Generacion_excel_envio_correo(pLinea, pUnidadMapeo, iOK, iKO, pPeriodo, sOutPut, sMsgError)
        ManejaError(iError, sMsgError, False)
        If iOK > 0 Then
            objLNGenerador.EnviaMail(pPeriodo, from, Destinatario1)
            objLNGenerador.EnviaMail(pPeriodo, from, Destinatario2)
        End If
        LogProceso("*******************************************************************")

        'Paso 4: Cerramos el archivo LOG
        iError = objLNGenerador.ParametrosInicioProceso(sFechaFinProceso, sNombreArchivoLOG, sMsgError)
        ManejaError(iError, sMsgError, True)

        GrabarPieArchivo()
        MostrarPiePantalla()
        iError = utilCerrarArchivo(sMsgError)
        ManejaError(iError, sMsgError, False)
    End Sub

    Private Sub LogProceso(ByVal pTexto As String)
        ' Escribimos en el archivo
        iError = utilEscribir(pTexto, sMsgError)
        ' Mostramos en pantalla
        Console.WriteLine(pTexto)
    End Sub

    Private Sub MostrarCabeceraPantalla()
        Console.WriteLine("*******************************************************************")
        Console.WriteLine("**** Generador de Administración de Cuentas Padres de Familia******")
        Console.WriteLine("*******************************************************************")
        Console.WriteLine("**Fecha inicio: " & sFechaIniProceso)
        Console.WriteLine("                                                  ")
    End Sub

    Private Sub GrabarCabeceraArchivo()
        iError = utilEscribir("*******************************************************************", sMsgError)
        iError = utilEscribir("**** Generador de Administración de Cuentas Padres de Familia******", sMsgError)
        iError = utilEscribir("*******************************************************************", sMsgError)
        iError = utilEscribir("**Fecha inicio: " & sFechaIniProceso, sMsgError)
        iError = utilEscribir("                                                  ", sMsgError)
    End Sub

    Private Sub GrabarPieArchivo()
        iError = utilEscribir("*******************************************************************", sMsgError)
        iError = utilEscribir("**Fecha término: " & sFechaFinProceso, sMsgError)
        iError = utilEscribir("*******************************************************************", sMsgError)
    End Sub

    Private Sub MostrarPiePantalla()
        Console.WriteLine("*******************************************************************")
        Console.WriteLine("**Fecha término: " & sFechaFinProceso)
        Console.WriteLine("*******************************************************************")
    End Sub

    Private Sub ManejaError(ByVal pCodError As Integer, ByVal pError As String, ByVal pExitApp As Boolean)
        If pCodError <> 0 Then
            Console.WriteLine(pError)
            If pExitApp Then
                Console.WriteLine("El sistema se detendrá.")
                End
            End If
        End If
    End Sub

End Module
