
Imports System.DirectoryServices
Imports ActiveDs
Imports CDO
Imports System.IO
Imports System.Net


Public Class LNGenerador

    Private connOracleBD As String
    Private connOracleBD2 As String
    Private connSpringBD As String

    Private dsSolicitud As DataTable
    Private rowSolicitud As DataRow

    Private dsContacto As DataTable
    Private rowContacto As DataRow


    Private objADGenerador As ADGENCUENPPFF.ADGenerador

    Public Sub New(ByVal pConnOracle As String, ByVal pConnOracle2 As String, _
                   ByVal pConnSpring As String)
        connOracleBD = pConnOracle
        connOracleBD2 = pConnOracle2
        connSpringBD = pConnSpring

        objADGenerador = New ADGENCUENPPFF.ADGenerador(connOracleBD, connOracleBD2, connSpringBD)
    End Sub

    Private Function valStr(ByVal obj As Object) As String
        If IsDBNull(obj) Then
            Return String.Empty
        Else
            Return Trim(obj).ToUpper
        End If
    End Function

    Public Function ParametrosInicioProceso(ByRef pFechaIniProceso As String, _
                                            ByRef pNombreArchivoLOG As String, _
                                            ByRef pMsgError As String) As Integer
        Return objADGenerador.ParametrosInicioProceso(pFechaIniProceso, pNombreArchivoLOG, pMsgError)
    End Function

    Private Sub ExecProcess(ByVal pIdSol As String, ByVal pExeName As String, ByVal pArgs As String, _
                            ByRef pCodError As Integer, ByRef pOut As String)
        Dim pr As Process
        Dim vStandardError As String
        Dim vStandardOutput As String
        Dim pExiste As Integer

        pr = New Process()
        pr.StartInfo.FileName = pExeName
        pr.StartInfo.Arguments = pArgs
        pr.StartInfo.UseShellExecute = False
        pr.StartInfo.RedirectStandardOutput = True
        pr.StartInfo.RedirectStandardError = True
        pr.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        pr.Start()
        vStandardOutput = pr.StandardOutput.ReadToEnd
        vStandardError = pr.StandardError.ReadToEnd
        If Convert.ToString("" & vStandardOutput) <> "" Then
            pOut = vStandardOutput & vbCrLf
            pCodError = 0
        ElseIf Convert.ToString("" & vStandardError) <> "" Then
            '*******************************************************************************
            '******Valida que el error sea porque ya existe el usuario en el Exchange*******
            '*******************************************************************************
            pExiste = InStr(vStandardError, "exists")
            If pExiste > 0 Then
                pOut = "La cuenta que solicita ya se encuentra creada en el AD."
                pCodError = 1
            Else
                pOut = "No se pudo ejecutar el comando " & pExeName & " " & pArgs & " " & vStandardError
                pCodError = 2
            End If
        Else
            pCodError = 0
            pOut = ""
        End If
        pr.WaitForExit()
        pr.Dispose()
    End Sub

    Public Function SP_PROCESAR_CUENTA_ALUM(ByVal pLinea As String, ByVal pModal As String, ByRef pPeriodo As String, ByRef vOK As Integer) As String
        Try
            Return objADGenerador.SP_PROCESAR_CUENTA_ALUM(pLinea, pModal, pPeriodo, vOK)
        Catch ex As Exception
        End Try
    End Function

    Public Function SP_PROCESAR_DEL_CUENTA_ALUM(ByVal pLinea As String, ByRef vOK As Integer) As String
        Try
            Return objADGenerador.SP_PROCESAR_DEL_CUENTA_ALUM(pLinea, vOK)
        Catch ex As Exception
        End Try
    End Function

    Public Sub NotificarVencimientoVigencia(ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String)
        Try
            objADGenerador.NotificarVencimientoVigencia(pLinea, pCodError, pMsgError)
        Catch ex As Exception

        End Try
    End Sub

    Public Function ProcesarCuentasADExchange(ByVal pLinea As String, ByVal pperiodo As String, ByVal pMapeo As String, ByRef pOK As Integer, ByRef pKO As Integer, _
                                              ByRef pOutput As String, ByRef pMsgError As String, ByVal pRuta As String) As Integer
        Dim iError As Integer
        Dim bPasoOk As Boolean
        Dim enviocorreo As Boolean = False

        Dim idSolicitud As String
        Dim codUsuario As String
        Dim sPassword As String = ""
        Dim codPersona As String
        Dim apellidoPatern As String
        Dim apellidoMatern As String
        Dim nombres As String

        Dim apellidos As String
        Dim nombrecompleto As String

        Dim cadena As String
        Dim textoVigencia As String
        Dim pOutput1 As String = ""
        Dim enter As String = Chr(10) '& Chr(13)


        Dim existe As Boolean


        'Dim iError2 As Integer      '*** Se crea para que afecte el valor iError = 1 que detiene el flujo.
        Dim pOutput2 As String = "" '*** Para que no se blanquee la cadena retornada.

        ' Obtenemos las solicitudes a las que debe crearse el usuario AD y Exchange
        dsSolicitud = objADGenerador.ObtenerSolCuentasADExchange(pLinea, pperiodo, iError, pOutput)

        If iError = 1 Then
            pMsgError = pOutput
            Return iError
        End If

        For Each rowSolicitud In dsSolicitud.Rows
            bPasoOk = True
            ' ******************************************

            'id_solicitud, cod_usuario , password, cod_persona, apellido_patern, apellido_matern, nombres

            idSolicitud = valStr(rowSolicitud("ID_SOLICITUD"))
            codUsuario = valStr(rowSolicitud("COD_USUARIO"))
            sPassword = valStr(rowSolicitud("PASSWORD"))
            apellidoPatern = valStr(rowSolicitud("APELLIDO_PATERN"))
            apellidoMatern = valStr(rowSolicitud("APELLIDO_MATERN"))
            nombres = valStr(rowSolicitud("NOMBRES"))

            apellidos = apellidoPatern.Trim & " " & apellidoMatern.Trim
            nombrecompleto = apellidoPatern.Trim & " " & apellidoMatern.Trim & ", " & nombres.Trim

            codPersona = valStr(rowSolicitud("COD_PERSONA"))

            cadena = ""
            pOutput = ""
            pOutput1 = ""

            ' ******************************************

            ' Paso 0: Obtengo los parametros 
            existe = False
            If codUsuario.Trim = "" Then
                iError = 1
                pOutput = "No se ha especificado código de usuario"
            Else
                'user cn=u201222619,OU=AlumnosMON,OU=Monterrico,OU=Usuarios,DC=upc,DC=edu,DC=pe -pwd 10139531 -fn "Nataly" -ln "Revilla Ruiz" -display "u201222619 (Revilla Ruiz, Nataly)" -desc "Usuario Alumno" -mustchpwd yes -upn u201222619@upc.edu.pe
                cadena = "user cn=" & codUsuario.Trim & "," & pRuta & " -pwd " & sPassword.Trim & " -fn " & Chr(34) & nombres.Trim & Chr(34) & " -ln " & Chr(34) & apellidos & Chr(34) & " -display " & Chr(34) & codUsuario.Trim & " (" & nombrecompleto.Trim & ")" & Chr(34) & " -desc " & Chr(34) & "Padre Familia" & Chr(34) & " -mustchpwd yes -upn " & codUsuario.Trim & "@upc.edu.pe"
            End If

            If iError = 0 Then

                ' Paso 1: Creo la cuenta en AD
                Try
                    ExecProcess(idSolicitud, "dsadd.exe", cadena, iError, pOutput1)
                    'iError = 0
                    If iError = 0 Then
                        objADGenerador.ActualizaSolicitud(idSolicitud, _
                                                          pLinea, 1, "", 2, Nothing, pOutput, "SI")
                    End If

                    If iError = 1 Then '***** ya existe
                        existe = True
                        objADGenerador.ActualizaSolicitud(idSolicitud, _
                                                          pLinea, 1, "", 1, 3, pOutput, Nothing)
                        iError = 0
                        pOutput = ""
                    ElseIf iError = 2 Then 'error 
                        pOutput = pOutput & vbCrLf & pOutput1
                    End If


                Catch ex As Exception
                    iError = 1
                    If pOutput = "" Then
                        pOutput = "No se pudo crear la cuenta en el AD: " & ex.Message
                    Else
                        pOutput = pOutput & vbCrLf & "No se pudo crear la cuenta en el AD: " & ex.Message
                    End If
                End Try
                bPasoOk = (iError = 0)

                ' Paso 2: Actualizo la vigencia del usuario
                If bPasoOk Then
                    bPasoOk = (iError = 0)
                    Try

                        cadena = "user CN=" & codUsuario & "," & pRuta & " -acctexpires never"
                        ' Ejecutamos el proceso
                        ExecProcess(idSolicitud, "dsmod.exe", cadena, iError, pOutput1)
                        'iError = 0
                        textoVigencia = "ilimitada"
                    Catch ex As Exception
                        iError = 0 ' No se debe de caer la solicitud porque no pudo asociar grupo de red
                        If pOutput = "" Then
                            pOutput = "No se pudo actualizar la vigencia del alumno " & Convert.ToString("" & codUsuario) & ": " & ex.Message
                        Else
                            pOutput = pOutput & vbCrLf & "No se pudo actualizar la vigencia del alumno " & Convert.ToString("" & codUsuario) & ": " & ex.Message
                        End If
                    End Try
                    bPasoOk = (iError = 0)
                End If

                ' Paso 3: Actualiza estado de solicitud y se envia correo al alumno
                If bPasoOk Then
                    'OK
                    objADGenerador.ActualizaSolicitud(idSolicitud, _
                                                      pLinea, 1, "", 1, 3, "OK", Nothing)
                    'objADGenerador.EnviarCorreoAlumno(idSolicitud, _
                    '                                  pLinea, 1, "")

                    enviocorreo = True
                End If

                If bPasoOk Then
                    pOK = pOK + 1
                    If pOutput = "" Then
                        If Not existe Then
                            pMsgError = pMsgError & vbCrLf & "- Se creó la cuenta AD y Exchange para el usuario " & codUsuario & " de la solicitud Nº " & idSolicitud & ", la vigencia de la cuenta es aprox. " & textoVigencia
                        Else
                            pMsgError = pMsgError & vbCrLf & "- Ya existe la cuenta AD y Exchange para el usuario " & codUsuario & " de la solicitud Nº " & idSolicitud & ", la vigencia de la cuenta es aprox. " & textoVigencia
                        End If

                    Else
                        If Not existe Then
                            pMsgError = pMsgError & vbCrLf & "- Se creó la cuenta AD y Exchange para el usuario " & codUsuario & " de la solicitud Nº " & idSolicitud & ", la vigencia de la cuenta es " & textoVigencia & pOutput
                        Else
                            pMsgError = pMsgError & vbCrLf & "- Ya existe la cuenta AD y Exchange para el usuario " & codUsuario & " de la solicitud Nº " & idSolicitud & ", la vigencia de la cuenta es " & textoVigencia & pOutput
                        End If
                    End If
                Else
                    pKO = pKO + 1
                    If pOutput.Trim = "False" Then
                        pOutput = "Se tuvieron inconvenientes con el servidor exchange. Se reintentará en la siguiente ejecución."
                    End If
                    pMsgError = pMsgError & vbCrLf & "- No se pudo crear la cuenta para la solicitud Nº " & idSolicitud & ": " & pOutput
                    objADGenerador.ActualizaSolicitud(idSolicitud, _
                                                      pLinea, 1, "", 1, 2, pOutput, Nothing)
                End If
            Else
                pKO = pKO + 1
                pMsgError = pMsgError & vbCrLf & "- No se pudo crear la cuenta para la solicitud Nº " & idSolicitud & ":  Se tuvieron inconvenientes con el servidor exchange. Se reintentará en la siguiente ejecución."
                objADGenerador.ActualizaSolicitud(idSolicitud, _
                                                      pLinea, 1, "", 1, 2, pOutput, Nothing)
            End If
            'Se setea el valor del iError luego de cada consulta. CSC-00262974-00
            iError = 0
        Next

        '' Paso 7: Se envia correo a SA y CI
        'If enviocorreo Then
        '    objADGenerador.EnviarCorreoSA_CI(pLinea, iError, pOutput)
        'End If

        Return iError
    End Function

    Public Function Generacion_excel_envio_correo(ByVal pLinea As String, ByVal pMapeo As String, ByRef pOK As Integer, ByRef pKO As Integer, ByVal pPeriodo As String, _
                                                  ByRef pOutput As String, ByRef pMsgError As String) As Integer

        Dim iError As Integer
        Dim bPasoOk As Boolean

        ' Obtenemos las solicitudes a las que debe crearse el usuario AD y Exchange
        dsSolicitud = objADGenerador.ObtenerSolCuentasArchivo(pLinea, pPeriodo, iError, pOutput)

        If iError = 1 Then
            pMsgError = pOutput
            Return iError
        End If

        'For Each rowSolicitud In dsSolicitud.Rows
        '    bPasoOk = True
        'Next

        'dsSolicitud.WriteXml("C:\archivo.xls")
        pOK = dsSolicitud.Rows.Count
        If pOK > 0 Then
            ExportarExcel(dsSolicitud, pPeriodo)
        End If
    End Function

    Function ExportarExcel(ByVal DT As DataTable, ByVal pperiodo As String) As Boolean
        'Creamos las variables
        Dim exApp As New Microsoft.Office.Interop.Excel.Application
        Dim exLibro As Microsoft.Office.Interop.Excel.Workbook
        Dim exHoja As Microsoft.Office.Interop.Excel.Worksheet
        Try
            'Añadimos el Libro al programa, y la hoja al libro
            exLibro = exApp.Workbooks.Add
            exHoja = exLibro.Worksheets.Add()
            exHoja.Name = "Reporte de Usuarios"
            ' ¿Cuantas columnas y cuantas filas?
            Dim NCol As Integer = DT.Columns.Count
            Dim NRow As Integer = DT.Rows.Count
            'Aqui recorremos todas las filas, y por cada fila todas las columnas y vamos escribiendo.
            For i As Integer = 1 To NCol
                exHoja.Cells.Item(1, i) = DT.Columns(i - 1).ColumnName.ToString
                'exHoja.Cells.AutoFormat(vFormato)
            Next
            For Fila As Integer = 0 To NRow - 1
                For Col As Integer = 0 To NCol - 1
                    exHoja.Cells.Item(Fila + 2, Col + 1) = DT.Rows(Fila).Item(Col).ToString()
                Next
            Next
            'Titulo en negrita, Alineado al centro y que el tamaño de la columna se ajuste al texto
            exHoja.Rows.Item(1).Font.Bold = 1
            exHoja.Rows.Item(1).HorizontalAlignment = 3
            exHoja.Columns.AutoFit()
            'Aplicación visible
            'exApp.Application.Visible = True

            exLibro.SaveAs("C:\EnviocartasPPFF" & pperiodo & ".xls", AccessMode:=Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, ConflictResolution:=Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlLocalSessionChanges)
            'exLibro.SaveAs("C:\EnviocartasPPFF" & pperiodo & ".xls", Type.Missing, Type.Missing, Type.Missing, True, False, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Microsoft.Office.Interop.Excel.XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing)
            exLibro.Close(True)

            exHoja = Nothing
            exLibro = Nothing
            exApp = Nothing
        Catch ex As Exception
            'MsgBox(ex.Message, MsgBoxStyle.Critical, Error al exportar a Excel )
            Return False
        End Try
        Return True
    End Function

    Public Function EnviaMail(ByVal pperiodo As String, ByVal de As String, ByVal destinatario As String) As Boolean
        Dim enter As String = Chr(10)
        Try
            Dim message As New System.Net.Mail.MailMessage
            message.From = New System.Net.Mail.MailAddress(de)

            message.To.Add(New System.Net.Mail.MailAddress(destinatario))
            message.Subject = "Creacion de cuentas PPFF " & pperiodo
            message.Body = "Estimado (a) usuario" & enter & " Se adjunta el resultado del proceso de creación de cuentas PPFF del periodo " & pperiodo & "." & enter & "Dirección de Sistemas"

            message.Attachments.Add(New System.Net.Mail.Attachment("C:\EnviocartasPPFF" & pperiodo & ".xls"))

            Dim client As New System.Net.Mail.SmtpClient()
            client.Send(message)

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function




End Class