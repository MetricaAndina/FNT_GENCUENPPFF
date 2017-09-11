Public Class EESolicitud
    Private aID_SOLICITUD As String
    Private aFECHA_SOLICITUD As String
    Private aCOD_PERSONA_SOC As String
    Private aCOD_PERSONA_SPRING As String
    Private aCOD_PERFIL As String
    Private aUSUARIO_SOLICITANTE As String
    Private aCOD_USUARIO As String
    Private aSERVER As String

    Property ID_SOLICITUD() As String
        Get
            Return aID_SOLICITUD
        End Get
        Set(ByVal value As String)
            aID_SOLICITUD = value
        End Set
    End Property

    Property FECHA_SOLICITUD() As String
        Get
            Return aFECHA_SOLICITUD
        End Get
        Set(ByVal value As String)
            aFECHA_SOLICITUD = value
        End Set
    End Property

    Property COD_PERSONA_SOC() As String
        Get
            Return aCOD_PERSONA_SOC
        End Get
        Set(ByVal value As String)
            aCOD_PERSONA_SOC = value
        End Set
    End Property

    Property COD_PERSONA_SPRING() As String
        Get
            Return aCOD_PERSONA_SPRING
        End Get
        Set(ByVal value As String)
            aCOD_PERSONA_SPRING = value
        End Set
    End Property

    Property COD_PERFIL() As String
        Get
            Return aCOD_PERFIL
        End Get
        Set(ByVal value As String)
            aCOD_PERFIL = value
        End Set
    End Property

    Property USUARIO_SOLICITANTE() As String
        Get
            Return aUSUARIO_SOLICITANTE
        End Get
        Set(ByVal value As String)
            aUSUARIO_SOLICITANTE = value
        End Set
    End Property

    Property COD_USUARIO() As String
        Get
            Return aCOD_USUARIO
        End Get
        Set(ByVal value As String)
            aCOD_USUARIO = value
        End Set
    End Property

    Property SERVER() As String
        Get
            Return aSERVER
        End Get
        Set(ByVal value As String)
            aSERVER = value
        End Set
    End Property
End Class
