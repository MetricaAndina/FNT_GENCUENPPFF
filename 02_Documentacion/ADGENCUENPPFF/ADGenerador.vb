Imports System
Imports System.Data
Imports System.Data.OracleClient
Imports Oracle.ApplicationBlocks.Data

Public Class ADGenerador

    Private connOracleBD As String
    Private connOracleBD2 As String
    Private connSpringBD As String

    Public Sub New(ByVal pConnOracle As String, ByVal pConnOracle2 As String, ByVal pConnSpring As String)
        connOracleBD = pConnOracle
        connOracleBD2 = pConnOracle2
        connSpringBD = pConnSpring
    End Sub

    Public Function ParametrosInicioProceso(ByRef pFechaIniProceso As String, _
                                            ByRef pNombreArchivoLOG As String, _
                                            ByRef pMsgError As String) As Integer
        Dim cmd As New OracleCommand
        Dim iRes As Integer

        Dim conn As New OracleConnection(connOracleBD)
        conn.Open()
        cmd.Connection = conn
        cmd.CommandType = CommandType.StoredProcedure
        cmd.CommandText = "PQ_GEN_ADM_CUENTA_PPFF.SP_PARAMETROS_INICIO"

        Dim prmMyParam As OracleParameter

        prmMyParam = New OracleParameter
        prmMyParam.OracleType = OracleType.VarChar
        prmMyParam.Size = 50
        prmMyParam.Direction = ParameterDirection.Output
        prmMyParam.ParameterName = "PFECHA_INICIO"
        cmd.Parameters.Add(prmMyParam)

        prmMyParam = New OracleParameter
        prmMyParam.OracleType = OracleType.VarChar
        prmMyParam.Size = 100
        prmMyParam.Direction = ParameterDirection.Output
        prmMyParam.ParameterName = "PNOMBRE_ARCHIVO"
        cmd.Parameters.Add(prmMyParam)

        prmMyParam = New OracleParameter
        prmMyParam.OracleType = OracleType.Number
        prmMyParam.Size = 4
        prmMyParam.Direction = ParameterDirection.Output
        prmMyParam.ParameterName = "PCOD_ERROR"
        cmd.Parameters.Add(prmMyParam)

        prmMyParam = New OracleParameter
        prmMyParam.OracleType = OracleType.VarChar
        prmMyParam.Size = 200
        prmMyParam.Direction = ParameterDirection.Output
        prmMyParam.ParameterName = "PMSG_ERROR"
        cmd.Parameters.Add(prmMyParam)

        Try
            cmd.ExecuteNonQuery()

            pFechaIniProceso = Convert.ToString("" & cmd.Parameters(0).Value)
            pNombreArchivoLOG = Convert.ToString("" & cmd.Parameters(1).Value)
            iRes = Convert.ToInt32(cmd.Parameters(2).Value)
            pMsgError = Convert.ToString("" & cmd.Parameters(3).Value)
        Catch ex As Exception
            iRes = -1
            pMsgError = ex.Message
        Finally
            cmd.Dispose()
            conn.Close()
        End Try

        Return iRes
    End Function

    Public Function ObtenerSolCuentasADExchange(ByVal pLinea As String, ByVal pperiodo As String, ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_SOL_CTAS_ADEXCH", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("Vperiodo", OracleType.VarChar, 6)
            pAdd.Direction = ParameterDirection.Input
            pAdd.Value = pPeriodo
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ObtenerSolCuentasArchivo(ByVal pLinea As String, ByVal pPeriodo As String, ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_ARCHIVO_OK", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("Vperiodo", OracleType.VarChar, 6)
            pAdd.Direction = ParameterDirection.Input
            pAdd.Value = pPeriodo
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ObtenerSolCuentasELIMADExchange(ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_SOL_DEL_CTAS_ADEXCH", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ObtenerCAMBIO_CORREOS(ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_CAMBIO_CORREOS", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ObtenerSolIngTras(ByVal pTipoSolicitud As Integer, _
                                      ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection = New OracleConnection(connOracleBD)
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_SOLINGTRAS", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PC_TIPO_SOLICITUD", OracleType.Number, 1)
            pAdd.Value = pTipoSolicitud
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ObtenerSolCese(ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection = New OracleConnection(connOracleBD)
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_SOLCESE", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function SP_PROCESAR_CUENTA_ALUM(ByVal pLinea As String, ByVal pModal As String, ByRef pPeriodo As String, ByRef vcount As Integer) As String
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_PROCESAR_CUENTA_PPFF", oCx)

        Dim per As String
        If pPeriodo Is Nothing Or pPeriodo = "" Then
            per = "0"
        Else
            per = pPeriodo
        End If

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("v_cod_linea_negocio", OracleType.VarChar, 1)
            pAdd.Value = pLinea
            pAdd = .Parameters.Add("v_usuario_creacion", OracleType.VarChar, 10)
            pAdd.Value = "ADMCUEPPFF"
            pAdd = .Parameters.Add("v_cod_modal_est", OracleType.VarChar, 2)
            pAdd.Value = pModal
            pAdd = .Parameters.Add("v_cod_periodoadicional", OracleType.VarChar, 6)
            pAdd.Value = per
            pAdd = .Parameters.Add("PRESULTADO", OracleType.VarChar, 50)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCOUNT", OracleType.VarChar, 50)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("pperiodo", OracleType.VarChar, 50)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()
            vcount = Convert.ToString("" & Cmddd.Parameters.Item("PCOUNT").Value).Trim
            pPeriodo = Convert.ToString("" & Cmddd.Parameters.Item("pperiodo").Value).Trim
            Return Convert.ToString("" & Cmddd.Parameters.Item("PRESULTADO").Value).Trim

        Catch ex As System.Exception
            Return ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Function

    Public Function SP_PROCESAR_DEL_CUENTA_ALUM(ByVal pLinea As String, ByRef vcount As Integer) As String
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_PROCESAR_DEL_CUENTA_ALUM", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("v_cod_linea_negocio", OracleType.VarChar, 1)
            pAdd.Value = pLinea
            pAdd = .Parameters.Add("v_usuario_creacion", OracleType.VarChar, 10)
            pAdd.Value = "ADMCUEPPFF"
            pAdd = .Parameters.Add("PRESULTADO", OracleType.VarChar, 50)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCOUNT", OracleType.VarChar, 50)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()
            vcount = Convert.ToString("" & Cmddd.Parameters.Item("PCOUNT").Value).Trim
            Return Convert.ToString("" & Cmddd.Parameters.Item("PRESULTADO").Value).Trim

        Catch ex As System.Exception
            Return ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Function



    Public Sub ObtenerRutaAD(ByVal pLinea As String, _
                                       ByRef pCadena As String, ByRef pCodError As Integer, _
                                       ByRef pMsgError As String)
        Dim oCx As OracleConnection
        oCx = New OracleConnection(connOracleBD)

        Dim sqlOraParam(3) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PLINEA", OracleType.VarChar, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pLinea.Trim

            sqlOraParam(1) = New OracleParameter("PCADENA", OracleType.VarChar, 2000)
            sqlOraParam(1).Direction = ParameterDirection.Output

            sqlOraParam(2) = New OracleParameter("PCOD_ERROR", OracleType.Float, 4)
            sqlOraParam(2).Direction = ParameterDirection.Output

            sqlOraParam(3) = New OracleParameter("PC_OUT_RESULTADO", OracleType.Char, 400)
            sqlOraParam(3).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_RUTAAD", sqlOraParam)

            pCadena = "" & Convert.ToString("" & sqlOraParam(1).Value.ToString())
            pCodError = Convert.ToInt32(sqlOraParam(2).Value.ToString())
            pMsgError = "" & Convert.ToString("" & sqlOraParam(3).Value.ToString())

        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub Pago_1era_boleta(ByVal pLinea As String, _
                                       ByRef pId As String, _
                                       ByRef pResult As String)
        Dim oCx As OracleConnection
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim sqlOraParam(2) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PLINEA", OracleType.VarChar, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pLinea.Trim

            sqlOraParam(1) = New OracleParameter("P_ID", OracleType.Number, 15)
            sqlOraParam(1).Direction = ParameterDirection.Input
            sqlOraParam(1).Value = pId.Trim

            sqlOraParam(2) = New OracleParameter("PC_OUT_RESULTADO", OracleType.Char, 400)
            sqlOraParam(2).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_PAGO_1ERA_BOLETA", sqlOraParam)

            pResult = "" & Convert.ToString("" & sqlOraParam(2).Value.ToString())

        Catch ex As System.Exception
            pResult = "NO" 'ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Function ListadoOU(ByVal pLinea As String, ByVal pIdSolicitud As String, _
                              ByRef iError As Integer, ByRef pMsgError As String) As String
        Dim Cmddd As OracleCommand
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        oCx = New OracleConnection(connOracleBD)

        Dim dts As New DataSet
        Cmddd = New OracleCommand

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "select GT.TIPO_CONTENEDOR, GC.DESCRIPCION  " & _
                            "        from GES_CONTENEDOR_ALUM GC ,GES_TIPO_CONTENEDOR GT  " & _
                            "        where GC.COD_TIPO_CONTENEDOR = GT.COD_TIPO_CONTENEDOR " & _
                            "        AND GC.COD_LINEA_NEGOCIO = :PLINEA " & _
                            "        AND GT.TIPO_CONTENEDOR = 'OU' " & _
                            "        ORDER BY POSICION ASC "

            .Parameters.Add(New OracleParameter(":PLINEA", pLinea))
        End With

        Try
            iError = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Dim row As DataRow
            Dim sCadena As String = ""

            For Each row In dts.Tables(0).Rows
                sCadena = sCadena & "OU=" & row("DESCRIPCION") & ","
            Next
            Return sCadena
        Else
            Return ""
        End If
    End Function

    Public Function ListadoGruposRed(ByVal idSolicitud As String, ByVal pServer As String, _
                                     ByRef iError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "SELECT DISTINCT GG.NOMBRE " & _
                           "FROM GES_SOLICITUD_GRUPORED GS, GES_GRUPO_RED GG " & _
                           "WHERE GS.ID_SOLICITUD = :PID_SOL AND " & _
                           "      GS.COD_GRUPO_RED = GG.COD_GRUPO "

            .Parameters.Add(New OracleParameter(":PID_SOL", idSolicitud))
        End With

        Try
            iError = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ListadoGruposCorreo(ByVal idSolicitud As String, ByVal pServer As String, _
                                        ByRef iError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "SELECT GG.NOMBRE_GRUPO " & _
                           "FROM GES_SOLICITUD_GRUPOCORREO GS, GES_GRUPO_CORREO GG " & _
                           "WHERE GS.ID_SOLICITUD = :PID_SOL AND " & _
                           "      GS.COD_GRUPO_CORREO = GG.COD_GRUPO"

            .Parameters.Add(New OracleParameter(":PID_SOL", idSolicitud))
        End With

        Try
            iError = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function ListadoCarpetasCompartidas(ByVal idSolicitud As String, ByVal pServer As String, _
                                               ByRef iError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "SELECT gr.cod_grupo_red, gg.nombre, ur.letra_unidad, " & _
                           "       ur.carpeta, nvl(ur.flag_lectura, 'NO') flag_lectura, " & _
                           "	   nvl(ur.flag_escritura, 'NO') flag_escritura " & _
                           "FROM ges_solicitud_grupored gr, ges_grupo_red gg, " & _
                           "     ges_unidad_red ur " & _
                           "where gr.id_solicitud= :PID_SOL and " & _
                           "	  gg.cod_grupo=gr.cod_grupo_red and " & _
                           "      ur.cod_grupo = gg.cod_grupo " & _
                           "order by ur.letra_unidad asc"

            .Parameters.Add(New OracleParameter(":PID_SOL", idSolicitud))
        End With

        Try
            iError = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function HabilitarDirectorioHogar(ByVal pIdSolicitud As String, ByVal pCodPerfil As String, _
                                             ByVal pServer As String, ByRef pDirectorioHogar As String, _
                                             ByRef pDirectorioHogar1 As String, ByRef pAlias As String, _
                                             ByRef pCodError As Integer, ByRef pMsgError As String) As Boolean
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim res As Boolean

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBT_DIRECTORIO_HOGAR", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 15)
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PCOD_PERFIL", OracleType.Number, 15)
            pAdd.Value = pCodPerfil
            pAdd = .Parameters.Add("POBT_DIRECTORIO", OracleType.Char, 2)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PPATH_DIRECTORIO", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PPATH_DIRECTORIO1", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PALIAS_DIRECTORIO", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pDirectorioHogar = Convert.ToString("" & Cmddd.Parameters.Item("PPATH_DIRECTORIO").Value)
            pDirectorioHogar1 = Convert.ToString("" & Cmddd.Parameters.Item("PPATH_DIRECTORIO1").Value)
            pAlias = Convert.ToString("" & Cmddd.Parameters.Item("PALIAS_DIRECTORIO").Value)
            If Convert.ToString("" & Cmddd.Parameters.Item("POBT_DIRECTORIO").Value) = "SI" Then
                res = True
            Else
                res = False
            End If
            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            res = False
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        HabilitarDirectorioHogar = res
    End Function

    Public Sub EliminaUsuarioRolesSocrates(ByVal pCodUsuario As String, ByVal pServer As String, _
                                           ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_ELIMINA_USUARIO_ROLES", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 10)
            pAdd.Value = pCodUsuario
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub CreaUsuarioRolesSocrates(ByVal pIdSolicitud As String, ByVal pCodUsuario As String, _
                                        ByVal pCodPersona As String, ByVal pUsuarioSol As String, _
                                        ByVal pServer As String, ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_INSERTA_USUARIO", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 9)
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 10)
            pAdd.Value = pCodUsuario
            pAdd = .Parameters.Add("PCOD_PERSONA", OracleType.Number, 15)
            pAdd.Value = pCodPersona
            pAdd = .Parameters.Add("PCOD_USUARIOSOL", OracleType.VarChar, 10)
            pAdd.Value = pUsuarioSol
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim

            '*******Agregado por Fernando Ruiz*****************
            '*****Valida q la cuenta en Socrates ya exista*****
            Dim pExiste As Integer
            pExiste = InStr(pMsgError, "se encuentra activo en el sistema")
            If pExiste > 0 Then
                pCodError = 2
            End If
            '***************************************************

        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub ProcesaSolicitudes(ByVal pIdSolicitud As String, ByVal pCodUsuarioSol As String, _
                                  ByVal pServer As String, ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_ACTUALIZA_SOL_ADEXCH", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 9)
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PCOD_USUARIOSOL", OracleType.VarChar, 10)
            pAdd.Value = pCodUsuarioSol
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub ActualizaSolicitud(ByVal pIdSolicitud As String, _
                                  ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String, ByVal pTipo As String, ByVal pEstado As String, ByVal pObs As String, ByVal pIndAD As String)
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        'pTipo  1 = estado 2= password  

        ' 1 se actualiza estado y obs
        ' 2 se actualiza solo pw e indAD


        Dim sqlOraParam(7) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PTIPO", OracleType.Number, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pTipo


            sqlOraParam(1) = New OracleParameter("PESTADO_SOLICITUD", OracleType.Number, 15)
            sqlOraParam(1).Direction = ParameterDirection.Input
            sqlOraParam(1).Value = pEstado

            sqlOraParam(2) = New OracleParameter("POBSERVACION", OracleType.VarChar, 500)
            sqlOraParam(2).Direction = ParameterDirection.Input
            sqlOraParam(2).Value = pObs

            sqlOraParam(3) = New OracleParameter("PUSUARIO", OracleType.VarChar, 10)
            sqlOraParam(3).Direction = ParameterDirection.Input
            sqlOraParam(3).Value = "ADMCUEPPFF"

            sqlOraParam(4) = New OracleParameter("PIND_CREO_CUENTAAD", OracleType.VarChar, 2)
            sqlOraParam(4).Direction = ParameterDirection.Input
            sqlOraParam(4).Value = pIndAD

            sqlOraParam(5) = New OracleParameter("PID_SOLICITUD", OracleType.Number, 15)
            sqlOraParam(5).Direction = ParameterDirection.Input
            sqlOraParam(5).Value = pIdSolicitud

            sqlOraParam(6) = New OracleParameter("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            sqlOraParam(6).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_ACTUALIZAR_SOLICITUD", sqlOraParam)

            pMsgError = "" & Convert.ToString("" & sqlOraParam(7).Value.ToString())

            If pMsgError.Length = 0 Then
                pCodError = 0
            Else
                pCodError = 1
            End If
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub


    Public Sub NotificarVencimientoVigencia(ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet

        Dim sqlOraParam(1) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PLINEA", OracleType.Char, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pLinea

            sqlOraParam(1) = New OracleParameter("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            sqlOraParam(1).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_NOTIF_CADUC_VIGENCIA", sqlOraParam)

            pMsgError = "" & Convert.ToString("" & sqlOraParam(1).Value.ToString())

            If pMsgError.Length = 0 Then
                pCodError = 0
            Else
                pCodError = 1
            End If
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub ObtenerVigencia(ByVal pLinea As String, ByVal pCodAlumno As String, ByRef vigencia As Integer)
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet


        Dim sqlOraParam(3) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PLINEA", OracleType.Char, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pLinea


            sqlOraParam(1) = New OracleParameter("PALUMNO", OracleType.VarChar, 9)
            sqlOraParam(1).Direction = ParameterDirection.Input
            sqlOraParam(1).Value = pCodAlumno

            sqlOraParam(2) = New OracleParameter("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            sqlOraParam(2).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_VIGENCIA", sqlOraParam)

            vigencia = Integer.Parse(sqlOraParam(2).Value.ToString())

        Catch ex As System.Exception
            vigencia = -1
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub


    Public Sub EnviarCorreoAlumno(ByVal pIdSolicitud As String, _
                                  ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet

        Dim sqlOraParam(2) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PLINEA", OracleType.VarChar, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pLinea

            sqlOraParam(1) = New OracleParameter("PID_SOLICITUD", OracleType.Number, 15)
            sqlOraParam(1).Direction = ParameterDirection.Input
            sqlOraParam(1).Value = pIdSolicitud

            sqlOraParam(2) = New OracleParameter("PC_OUT_RESULTADO", OracleType.VarChar, 1000)
            sqlOraParam(2).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_ENVIAR_CORREO_ALUMNO", sqlOraParam)

            pMsgError = "" & Convert.ToString("" & sqlOraParam(2).Value.ToString())

            'If pMsgError.Length = 0 Then
            pCodError = 0
            'Else
            '    pCodError = 1
            'End If
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub EnviarCorreoSA_CI(ByVal pLinea As String, ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet

        Dim sqlOraParam(1) As OracleParameter
        Try

            sqlOraParam(0) = New OracleParameter("PLINEA", OracleType.VarChar, 1)
            sqlOraParam(0).Direction = ParameterDirection.Input
            sqlOraParam(0).Value = pLinea

            sqlOraParam(1) = New OracleParameter("PC_OUT_RESULTADO", OracleType.VarChar, 1000)
            sqlOraParam(1).Direction = ParameterDirection.Output

            OraHelper.ExecuteNonQuery(oCx, CommandType.StoredProcedure, "PQ_GEN_ADM_CUENTA_PPFF.SP_ENVIAR_CORREO_SA_CI", sqlOraParam)

            pMsgError = "" & Convert.ToString("" & sqlOraParam(1).Value.ToString())

            'If pMsgError.Length = 0 Then
            pCodError = 0
            'Else
            '    pCodError = 1
            'End If
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub


    Public Sub FinalizaSolicitudIngreso(ByVal pIdSolicitud As String, ByVal pCodUsuario As String, _
                                        ByVal pCodUsuarioSol As String, ByVal pServer As String, _
                                        ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If

        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_FINALIZA_PROCESO", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 9)
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 10)
            pAdd.Value = pCodUsuario
            pAdd = .Parameters.Add("PCOD_USUARIOSOL", OracleType.VarChar, 10)
            pAdd.Value = pCodUsuarioSol
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try
    End Sub

    Public Sub EliminaUsuarioSpring(ByVal pCodUsuario As String, ByVal pCodPersonaSprg As String, _
                                    ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = pCodUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_persona", SqlDbType.Int)
        objPar.Value = pCodPersonaSprg
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int, 4)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_borrar_usuario"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                pCodError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                pCodError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    Public Sub CreaUsuarioSpring(ByVal codPersonaSpring As String, ByVal codUsuario As String, _
                                 ByRef iError As Integer, ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@accion", SqlDbType.Char, 30)
        objPar.Value = "CREAR"
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_persona", SqlDbType.Int)
        objPar.Value = codPersonaSpring
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = codUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int, 4)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_usuario"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                iError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)

                '*******Agregado por Fernando Ruiz*****************
                '*****Valida q la cuenta en Socrates ya exista*****
                Dim pExiste As Integer, pexiste1 As Integer
                pExiste = InStr(pMsgError, "ya ha sido asignado en el pasado")
                pexiste1 = InStr(pMsgError, "ya existe en el maestro")
                If pExiste > 0 Or pexiste1 > 0 Then
                    iError = 2
                End If
                '***************************************************
            Else
                iError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    Public Sub InactivaUsuarioSpring(ByVal codPersonaSpring As String, ByVal codUsuario As String, _
                                     ByRef iError As Integer, ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@accion", SqlDbType.Char, 30)
        objPar.Value = "INACTIVAR"
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_persona", SqlDbType.Int)
        objPar.Value = codPersonaSpring
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = codUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int, 4)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_usuario"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                iError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                iError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    ' Inactiva todos los permisos, reportes y perfiles de un usuario
    Public Sub InactivaPermisosSpring(ByVal pCodUsuario As String, ByRef pCodError As Integer, _
                                      ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = pCodUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_inactivar_permisos_spring"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                pCodError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                pCodError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    ' Inactiva solo los permisos de un usuario (no los perfiles o reportes)
    Public Sub InactivaPermisosSpring(ByVal pCodUsuario As String, ByVal pCodAplicacion As String, _
                                      ByVal pCodGrupo As String, ByVal pCodConcepto As String, _
                                      ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = pCodUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_aplicacion", SqlDbType.Char, 2)
        objPar.Value = pCodAplicacion
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_grupo", SqlDbType.Char, 20)
        objPar.Value = pCodGrupo
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_concepto", SqlDbType.Char, 20)
        objPar.Value = pCodConcepto
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuariomodificador", SqlDbType.Char, 20)
        objPar.Value = "GENADMPERS"
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_inactivar_acceso_usuario"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                pCodError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                pCodError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    Public Sub GeneraPermisosSpring(ByVal pAccion As String, ByVal pCodUsuario As String, _
                                    ByVal pTipoAcceso As String, ByVal pCodAplicacion As String, _
                                    ByVal pCodGrupo As String, ByVal pCodConcepto As String, _
                                    ByVal pFlagAdd As String, ByVal pFlagEdit As String, _
                                    ByVal pFlagDel As String, ByVal pFlagAprob As String, _
                                    ByRef pCodError As Integer, ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@accion", SqlDbType.Char, 30)
        objPar.Value = pAccion
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@tipoacceso", SqlDbType.Char, 1)
        objPar.Value = pTipoAcceso
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = pCodUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_aplicacion", SqlDbType.Char, 2)
        objPar.Value = pCodAplicacion
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_grupo", SqlDbType.Char, 20)
        objPar.Value = pCodGrupo
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_concepto", SqlDbType.Char, 20)
        objPar.Value = pCodConcepto
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@flag_agregar", SqlDbType.Char, 1)
        objPar.Value = pFlagAdd
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@flag_modificar", SqlDbType.Char, 1)
        objPar.Value = pFlagEdit
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@flag_eliminar", SqlDbType.Char, 1)
        objPar.Value = pFlagDel
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@flag_aprobar", SqlDbType.Char, 1)
        objPar.Value = pFlagAprob
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuariomodificador", SqlDbType.Char, 20)
        objPar.Value = "GENADMPERS"
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_autorizar_acceso_usuario"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                pCodError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                pCodError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    ' Esta funcion permite obtener el listado de permisos de Spring desde Spring mismo
    Public Function ListaPermisosSpring(ByVal codUsuario As String, ByRef ierror As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As New SqlClient.SqlCommand
        Dim oCx As New SqlClient.SqlConnection(connSpringBD)
        Dim dts As New DataSet

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "select AplicacionCodigo, Grupo, Concepto " & _
                           "from SeguridadAutorizaciones " & _
                           "where Usuario='" & codUsuario & "' and Estado='A'"
        End With

        Try
            ierror = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As New SqlClient.SqlDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            ierror = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    ' Esta funcion permite obtener el listado de permisos de Spring desde Oracle
    Public Function ListaPermisosSpring(ByVal idSolicitud As String, ByVal codUsuario As String, _
                                        ByVal server As String, ByRef iError As Integer, _
                                        ByRef pMsgError As String) As DataTable
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If server = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBT_SPRING_CONCEPTO", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 9)
            pAdd.Value = idSolicitud
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            iError = 0
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Sub GeneraPerfilesSpring(ByVal accion As String, ByVal codUsuario As String, _
                                    ByVal perfil As String, ByRef iError As Integer, _
                                    ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@accion", SqlDbType.Char, 30)
        objPar.Value = accion
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = codUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@perfil", SqlDbType.Char, 20)
        objPar.Value = perfil
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int, 4)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_perfiles"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                iError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                iError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    ' Esta funcion obtiene el listado de perfiles Spring desde Spring mismo
    Public Function ListaPerfilesSpring(ByVal codUsuario As String, ByRef iError As Integer, _
                                        ByRef pMsgError As String) As DataTable
        Dim Cmddd As New SqlClient.SqlCommand
        Dim oCx As New SqlClient.SqlConnection(connSpringBD)
        Dim dts As New DataSet

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "select Perfil " & _
                           "from SeguridadPerfilUsuario " & _
                           "where Usuario='" & codUsuario & "' and Estado='A'"
        End With

        Try
            iError = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As New SqlClient.SqlDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    ' Esta funcion obtiene el listado de perfiles Spring desde Socrates
    Public Function ListaPerfilesSpring(ByVal idSolicitud As String, ByVal codUsuario As String, _
                                        ByVal server As String, ByRef iError As Integer, _
                                        ByRef pMsgError As String) As DataTable
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If server = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBT_SPRING_PERFILES", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 9)
            pAdd.Value = idSolicitud
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            iError = 0
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Sub GeneraReportesSpring(ByVal accion As String, ByVal codUsuario As String, _
                                    ByVal aplicacion As String, ByVal reporte As String, _
                                    ByRef iError As Integer, ByRef pMsgError As String)
        Dim connSQLServer As New SqlClient.SqlConnection(connSpringBD)
        Dim cmdSQLServer As New SqlClient.SqlCommand

        Dim objPar As SqlClient.SqlParameter
        objPar = New SqlClient.SqlParameter("@accion", SqlDbType.Char, 30)
        objPar.Value = accion
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@codigo_usuario", SqlDbType.Char, 20)
        objPar.Value = codUsuario
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@aplicacion", SqlDbType.Char, 2)
        objPar.Value = aplicacion
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@reporte", SqlDbType.Char, 3)
        objPar.Value = reporte
        objPar.Direction = ParameterDirection.Input
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("@msg_error", SqlDbType.Char, 255)
        objPar.Direction = ParameterDirection.Output
        cmdSQLServer.Parameters.Add(objPar)

        objPar = New SqlClient.SqlParameter("Return_Value", SqlDbType.Int, 4)
        objPar.Direction = ParameterDirection.ReturnValue
        cmdSQLServer.Parameters.Add(objPar)

        Try
            connSQLServer.Open()
            cmdSQLServer.Connection = connSQLServer
            cmdSQLServer.CommandType = CommandType.StoredProcedure
            cmdSQLServer.CommandText = "pa_sy_seguridad_reportes"
            cmdSQLServer.ExecuteNonQuery()
            If cmdSQLServer.Parameters.Item("Return_Value").Value = 1 Then
                iError = 1
                pMsgError = Convert.ToString("" & cmdSQLServer.Parameters.Item("@msg_error").Value)
            Else
                iError = 0
                pMsgError = ""
            End If
        Catch ex As Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            cmdSQLServer.Dispose()
            connSQLServer.Close()
            connSQLServer.Dispose()
        End Try
    End Sub

    ' Esta funcion obtiene el listado de reportes de Spring desde Spring mismo
    Public Function ListaReportesSpring(ByVal codUsuario As String, ByRef iError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As New SqlClient.SqlCommand
        Dim oCx As New SqlClient.SqlConnection(connSpringBD)
        Dim dts As New DataSet

        With (Cmddd)
            .CommandType = CommandType.Text
            .Connection = oCx
            .CommandText = "select AplicacionCodigo, Reportecodigo " & _
                           "from SeguridadAutorizacionReporte " & _
                           "where Usuario='" & codUsuario & "' and " & _
                           "	  Disponible='S'"
        End With

        Try
            iError = 0
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As New SqlClient.SqlDataAdapter(Cmddd)
            adapter.Fill(dts)
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    ' Esta funcion obtiene el listado de reportes de Spring desde Oracle
    Public Function ListaReportesSpring(ByVal idSolicitud As String, ByVal codUsuario As String, _
                                        ByVal server As String, ByRef iError As Integer, _
                                        ByRef pMsgError As String) As DataTable
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If server = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBT_SPRING_REPORTE", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 9)
            pAdd.Value = idSolicitud
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            iError = 0
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            iError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Public Function EnviarAvisosSolicitud(ByRef pFechaFin As String, ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        ' Se ejecuta siempre en PROD
        Dim oCx As New OracleConnection(connOracleBD)
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_ENVIAR_AVISOS_SOL", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PFECHA_FIN", OracleType.VarChar, 50)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pFechaFin = "" & Convert.ToString("" & Cmddd.Parameters.Item("PFECHA_FIN").Value)
            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function InactivarUsuarioSocrates(ByVal pServer As String, ByVal pCodusuario As String, ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_INACTIVAR_USUARIO", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 20)
            pAdd.Value = pCodusuario
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function FinalizarProcesoCese(ByVal pServer As String, ByVal pIdSolicitud As String, _
                                         ByVal pCodusuario As String, ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_FINALIZA_PROCESO_CESE", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 15)
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 20)
            pAdd.Value = pCodusuario
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function FinalizaSolicitudTraslado(ByVal idSolicitud As Integer, ByVal pCodUsuario As String, _
                                              ByVal pCodUsuarioSol As String, ByVal pServer As String, _
                                              ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_FINALIZA_PROCESO_TRAS", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 15)
            pAdd.Value = idSolicitud
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 20)
            pAdd.Value = pCodUsuario
            pAdd = .Parameters.Add("PCOD_USUARIOSOL", OracleType.VarChar, 20)
            pAdd.Value = pCodUsuarioSol
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function TrasladarUsuarioSocrates(ByVal idSolicitud As Integer, ByVal pCodUsuario As String, _
                                             ByVal pCodUsuarioSol As String, ByVal pServer As String, _
                                             ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_TRASLADAR_USUARIO_SOC", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 15)
            pAdd.Value = idSolicitud
            pAdd = .Parameters.Add("PCOD_USUARIO", OracleType.VarChar, 20)
            pAdd.Value = pCodUsuario
            pAdd = .Parameters.Add("PCOD_USUARIOSOL", OracleType.VarChar, 20)
            pAdd.Value = pCodUsuarioSol
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function ObtenerParametrosExchange(ByVal pIdSolicitud As Double, ByRef strFirstName As String, _
                                              ByRef strLastName As String, ByRef strUserName As String, _
                                              ByRef strContainerName As String, ByRef strHomeMDBUrl As String, _
                                              ByVal pLinea As String, ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        oCx = New OracleConnection(connOracleBD)
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBT_PARAM_EXCHANGE", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PCOD_PERSONA", OracleType.Number, 15)
            pAdd.Direction = ParameterDirection.Input
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PLINEA", OracleType.VarChar, 1)
            pAdd.Direction = ParameterDirection.Input
            pAdd.Value = pLinea
            pAdd = .Parameters.Add("PFNAME", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PLNAME", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PFULLNAME", OracleType.VarChar, 200)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCONTAINER", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PHOMEMDBURL", OracleType.VarChar, 3000)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_ERROR", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            strFirstName = "" & Convert.ToString(Cmddd.Parameters.Item("PFNAME").Value).Trim
            strLastName = "" & Convert.ToString(Cmddd.Parameters.Item("PLNAME").Value).Trim
            strUserName = "" & Convert.ToString(Cmddd.Parameters.Item("PFULLNAME").Value).Trim
            strContainerName = "" & Convert.ToString(Cmddd.Parameters.Item("PCONTAINER").Value).Trim
            strHomeMDBUrl = "" & Convert.ToString(Cmddd.Parameters.Item("PHOMEMDBURL").Value).Trim

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_ERROR").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function ObtenerParametrosExchange(ByVal pIdSolicitud As Integer, ByRef strFirstName As String, _
                                              ByRef strLastName As String, ByRef strUserName As String, _
                                              ByRef strContainerName As String, ByVal pServer As String, _
                                              ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Segun de donde se obtuvo la solicitud se leen los parametros
        If pServer = "PROD" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBT_PARAM_EXCHANGE2", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PID_SOLICITUD", OracleType.Number, 15)
            pAdd.Value = pIdSolicitud
            pAdd = .Parameters.Add("PFNAME", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PLNAME", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PFULLNAME", OracleType.VarChar, 200)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCONTAINER", OracleType.VarChar, 100)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_ERROR", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            strFirstName = "" & Convert.ToString(Cmddd.Parameters.Item("PFNAME").Value).Trim
            strLastName = "" & Convert.ToString(Cmddd.Parameters.Item("PLNAME").Value).Trim
            strUserName = "" & Convert.ToString(Cmddd.Parameters.Item("PFULLNAME").Value).Trim
            strContainerName = "" & Convert.ToString(Cmddd.Parameters.Item("PCONTAINER").Value).Trim

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_ERROR").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function EnviarAvisosControlOwners(ByRef pMsgError As String) As Integer
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        ' Se ejecuta siempre en PROD
        oCx = New OracleConnection(connOracleBD)
        Dim pCodError As Integer

        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_ENVIAR_AVISOS_COWNER", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PCOD_ERROR", OracleType.Number, 4)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            pCodError = Convert.ToInt32(Cmddd.Parameters.Item("PCOD_ERROR").Value)
            pMsgError = Convert.ToString("" & Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
        Catch ex As System.Exception
            pCodError = 1
            pMsgError = ex.Message
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        Return pCodError
    End Function

    Public Function ObtenerSolContactosElim(ByVal pLinea As String, ByVal persona As String, ByVal tipo As String, ByRef pCodError As Integer, ByRef pMsgError As String) As DataTable
        Dim Cmddd As OracleCommand
        Dim pAdd As OracleParameter
        Dim oCx As OracleConnection
        If pLinea = "U" Then
            oCx = New OracleConnection(connOracleBD)
        Else
            oCx = New OracleConnection(connOracleBD2)
        End If
        Dim dts As New DataSet
        Cmddd = New OracleCommand("PQ_GEN_ADM_CUENTA_PPFF.SP_OBTENER_CONTACTOS_DEL", oCx)

        With (Cmddd)
            .CommandType = CommandType.StoredProcedure
            pAdd = .Parameters.Add("PC_COD_PERSONA", OracleType.Number)
            pAdd.Direction = ParameterDirection.Input
            pAdd.Value = persona
            pAdd = .Parameters.Add("PC_COD_TIPO", OracleType.VarChar)
            pAdd.Direction = ParameterDirection.Input
            pAdd.Value = tipo
            pAdd = .Parameters.Add("PC_OUT_CURSOR", OracleType.Cursor)
            pAdd.Direction = ParameterDirection.Output
            pAdd = .Parameters.Add("PC_OUT_RESULTADO", OracleType.VarChar, 400)
            pAdd.Direction = ParameterDirection.Output
        End With

        Try
            oCx.Open()
            Cmddd.ExecuteNonQuery()

            Dim adapter As OracleDataAdapter
            adapter = New OracleDataAdapter(Cmddd)
            adapter.Fill(dts)
            pMsgError = "" & Convert.ToString(Cmddd.Parameters.Item("PC_OUT_RESULTADO").Value).Trim
            If Not pMsgError.Equals(String.Empty) Then
                Throw New Exception(pMsgError)
            End If
        Catch a As System.Exception
            Throw a
        Finally
            oCx.Close()
            oCx.Dispose()
        End Try

        If dts.Tables.Count > 0 Then
            Return dts.Tables(0)
        Else
            Return Nothing
        End If
    End Function

    Private Function valStr(ByVal obj As Object) As String
        If IsDBNull(obj) Then
            Return String.Empty
        Else
            Return Trim(obj).ToUpper
        End If
    End Function
End Class
