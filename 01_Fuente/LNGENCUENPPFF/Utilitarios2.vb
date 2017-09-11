Imports System.IO
Imports System.Configuration

Module Utilitarios2
    Private oSW As StreamWriter

    ' Funcion que crea un archivo de texto para almacenar el log del proceso
    ' Usa el nombre pArchivo.txt y devuelve 0 = ok u -1 = error
    Public Function utilCrearArchivo2(ByVal pArchivo As String, ByVal pUnidad As String, ByRef pMsgError As String) As Integer
        Try
            If File.Exists(pUnidad & pArchivo) Then
                File.Delete(pUnidad & pArchivo)
            End If

            ' Creamos el archivo
            oSW = New StreamWriter(pUnidad & pArchivo)
            Return 0
        Catch ex As Exception
            pMsgError = "Error al crear el archivo: " & ex.Message
            Return -1
        End Try
    End Function

    Public Function utilCerrarArchivo2(ByRef pMsgError As String) As Integer
        Try
            ' Cerramos el archivo
            oSW.Close()
            oSW.Dispose()
            Return 0
        Catch ex As Exception
            pMsgError = "Error al cerrar el archivo: " & ex.Message
            Return -1
        End Try
    End Function

    Public Function utilEscribir2(ByVal pTexto As String, ByRef pMsgError As String) As Integer
        Try
            oSW.WriteLine(pTexto)
            oSW.Flush()
            Return 0
        Catch ex As Exception
            pMsgError = "Error al escribir en el archivo: " & ex.Message
            Return -1
        End Try
    End Function

End Module
