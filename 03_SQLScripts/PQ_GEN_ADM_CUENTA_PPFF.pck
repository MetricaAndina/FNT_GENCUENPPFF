create or replace package PQ_GEN_ADM_CUENTA_PPFF is
  -- Author  : RBOGOVIC
  -- Created : 18-02-2013 04:04:16 PM
  -- Purpose : PAQUETE PARA EL PROCESO DE CREACION DE USUARIOS PADRES DE FAMILIA
  -- Public type declarations
  TYPE I_CURSOR IS REF CURSOR;
/***************************************************************************************************/
PROCEDURE SP_PARAMETROS_INICIO(PFECHA_INICIO   OUT VARCHAR2,
                               PNOMBRE_ARCHIVO OUT VARCHAR2,
                               PCOD_ERROR       OUT NUMBER,
                               PMSG_ERROR       OUT VARCHAR2);
/***************************************************************************************************/
PROCEDURE SP_PROCESAR_CUENTA_PPFF(v_cod_linea_negocio in linea_negocio.cod_linea_negocio%type,
                                  v_usuario_creacion in usuario.cod_usuario%type,
                                  v_cod_modal_est in modalidad_estud.cod_modal_est%type,
                                  v_cod_periodoadicional in periodo.cod_periodo%type,
                                  PRESULTADO out varchar2,
                                  PCOUNT out varchar2,
                                  pperiodo out varchar2
                                 );
/***************************************************************************************************/
PROCEDURE SP_OBTENER_SOL_CTAS_ADEXCH(Vperiodo periodo.cod_periodo%type,
                                     PC_OUT_CURSOR     OUT I_CURSOR,
                                     PC_OUT_RESULTADO  OUT VARCHAR2);
/***************************************************************************************************/
PROCEDURE SP_OBTENER_ARCHIVO_OK(Vperiodo periodo.cod_periodo%type,
                                PC_OUT_CURSOR     OUT I_CURSOR,
                                PC_OUT_RESULTADO  OUT VARCHAR2);
/***************************************************************************************************/
FUNCTION SF_OBTENER_USUARIO_PPFF(V_COD_LINEA_NEGOCIO IN VARCHAR2,V_APE_PATERN IN VARCHAR2, V_APE_MATERN IN VARCHAR2, VNOMB IN VARCHAR2
  ) return varchar2;
/***************************************************************************************************/
PROCEDURE SP_ACTUALIZAR_SOLICITUD(
                                   PTIPO            IN NUMBER,
                                   PESTADO_SOLICITUD IN GES_SOLICITUD_ALUM.ESTADO_SOLICITUD%TYPE,
                                   POBSERVACION IN GES_SOLICITUD_ALUM.OBSERVACION%TYPE,
                                   PUSUARIO IN GES_SOLICITUD_ALUM.USUARIO_CREACION%TYPE,
                                   PIND_CREO_CUENTAAD IN GES_SOLICITUD_ALUM.IND_CREO_CUENTAAD%TYPE,
                                   PID_SOLICITUD    IN GES_SOLICITUD_ALUM.ID_SOLICITUD%TYPE,
                                   PC_OUT_RESULTADO OUT VARCHAR2);
/***************************************************************************************************/
end PQ_GEN_ADM_CUENTA_PPFF;
/
create or replace package body PQ_GEN_ADM_CUENTA_PPFF is
/***************************************************************************************************/
PROCEDURE SP_PARAMETROS_INICIO(PFECHA_INICIO   OUT VARCHAR2,
                                 PNOMBRE_ARCHIVO OUT VARCHAR2,
                               PCOD_ERROR       OUT NUMBER,
                               PMSG_ERROR       OUT VARCHAR2)
IS
BEGIN
     PCOD_ERROR := 0;
     PMSG_ERROR := '';
     SELECT TO_CHAR(SYSDATE, 'DD/MM/YYYY HH:MI:SS AM'),
            'LOGGENADMPPFF'||TO_CHAR(SYSDATE, 'DDMMYYYYHHMISS')||'.txt'
        INTO PFECHA_INICIO, PNOMBRE_ARCHIVO
     FROM DUAL;
EXCEPTION
   WHEN OTHERS THEN
           PCOD_ERROR := -1;
END;
/***************************************************************************************************/
PROCEDURE SP_PROCESAR_CUENTA_PPFF(v_cod_linea_negocio in linea_negocio.cod_linea_negocio%type,
                                  v_usuario_creacion in usuario.cod_usuario%type,
                                  v_cod_modal_est in modalidad_estud.cod_modal_est%type,
                                  v_cod_periodoadicional in periodo.cod_periodo%type,
                                  PRESULTADO out varchar2,
                                  PCOUNT out varchar2,
                                  pperiodo out varchar2
                                 )
is
v_cod_periodo periodo.cod_periodo%type;
VUSUARIO USUARIO.COD_USUARIO%TYPE;
V NUMBER(10);
VV NUMBER(10);
westado_solicitud number(2);
WPASSWORD varchar(10);
WUSUARIO_ANT number(2);
WBASEANTIGUA number(2);
valor_antiguo number(2);
begin
if (v_cod_periodoadicional is null) or (v_cod_periodoadicional = '0') then
begin
select cod_periodo into v_cod_periodo
from periodo
where cod_linea_negocio = v_cod_linea_negocio
and cod_modal_est = v_cod_modal_est
and fecha_inicio =
(
select max(fecha_inicio) from periodo
where cod_linea_negocio = v_cod_linea_negocio
and cod_modal_est = v_cod_modal_est
);
pperiodo :=  v_cod_periodo;
exception when others then
  v_cod_periodo := null;
  PRESULTADO := '0';
  pcount := 0;
end;
else
v_cod_periodo := v_cod_periodoadicional;
pperiodo :=  v_cod_periodo;
end if;
if v_cod_periodo is not null then
PCOUNT := 0;
/* Control de Cambios
Ticket : CSC-00262669-00
Rdepaz 29/09/2015
Se modifica para que se consideren alumnos con hermanos y se asocie al apoderado antiguo
Si el usuario ya existe sólo debe registrarse en el excel para el envio de cartas pero
no debe crearse nuevamente el usuario en el AD
*/
for x in
(
    select
    PASSWORD ,
    COD_ALUMNO,
    COD_PERSONA_H,
    APELLIDO_PATERN_H,
    APELLIDO_MATERN_H,
    NOMBRES_H,
    SEXO_H,
    DETALLE_PARIENTE,
    COD_PERSONA,
    APELLIDO_PATERN,
    APELLIDO_MATERN,
    NOMBRES,
    PERSONA_FECHA_NACIMIENT,
    VIA,
    DIRECCION,
    URBANIZACION,
    DEPARTAMENTO,
    PROVINCIA,
    DISTRITO,
    SEXO,
  -- CSC-00261922-00  JDELACRU
    --decode(DETALLE_PARIENTE,'PADRE',1, 'MADRE',2,'APODERADO',3) orden
  DETALLE_PARIENTE orden
  -- CSC-00261922-00  JDELACRU
    from
    (
    select
     POSTULANTE.COD_ALUMNO     ,
          POSTULANTE.COD_PERSONA    cod_persona_h,
              PERSONA.APELLIDO_PATERN       apellido_patern_h,
              PERSONA.APELLIDO_MATERN       apellido_matern_h,
              PERSONA.NOMBRES               nombres_h,
              PERSONA.SEXO                  sexo_h,--
          PARENT_PADRE.DETALLE_PARIENTE,
              PADRE.COD_PERSONA             ,
              PADRE.APELLIDO_PATERN         ,
              PADRE.APELLIDO_MATERN         ,
              PADRE.NOMBRES                 ,
          DECODE ( PERSONA.FECHA_NACIMIENT, NULL, '00000000', TO_CHAR(PERSONA.FECHA_NACIMIENT,'DDMMYYYY'))  PERSONA_FECHA_NACIMIENT       ,
              (SELECT DESCRIPCION FROM VIA WHERE COD_VIA = PERSONA.COD_VIA) VIA,--I
              PERSONA.DIRECCION               ,
              PERSONA.URBANIZACION            ,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO))) DEPARTAMENTO,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO)) PROVINCIA,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO) DISTRITO,
              PADRE.SEXO,
              DECODE(
              (
              SELECT COUNT(1)
              FROM PARENTESCO PP
              WHERE COD_PARIENTE = PADRE.COD_PERSONA
              AND DETALLE_PARIENTE IN ('1','2','3') --('PADRE', 'MADRE','APODERADO') -- CSC-00261922-00  JDELACRU
              and exists (SELECT A.COD_PERSONA FROM MATRICULA M, ALUMNO A
                          WHERE M.COD_LINEA_NEGOCIO = A.COD_LINEA_NEGOCIO
                          AND M.COD_ALUMNO = A.COD_ALUMNO
                          AND A.COD_PERSONA = PP.COD_PERSONA
                          AND M.COD_LINEA_NEGOCIO = v_cod_linea_negocio
                          AND M.COD_MODAL_EST = v_cod_modal_est
                          AND M.COD_PERIOD_MAT = v_cod_periodo
                          )
              ), 1, DECODE ( PERSONA.FECHA_NACIMIENT, NULL, '12345678', TO_CHAR(PERSONA.FECHA_NACIMIENT,'DDMMYYYY')) ,'12345678') PASSWORD
         FROM POSTULANTE POSTULANTE ,
              PERSONA PERSONA ,
              PERSONA PADRE ,
              PARENTESCO PARENT_PADRE ,
          MATRICULA  MATRICUL
        WHERE PERSONA.COD_PERSONA              = POSTULANTE.COD_PERSONA AND
              PARENT_PADRE.COD_PERSONA      = PERSONA.COD_PERSONA AND
              -- CSC-00261922-00  JDELACRU
              PARENT_PADRE.DETALLE_PARIENTE = '1' AND --'PADRE' AND
              -- CSC-00261922-00  JDELACRU
      PADRE.COD_PERSONA             = PARENT_PADRE.COD_PARIENTE AND
          PADRE.ESTADO_ACTIVO      ='SI' AND
          POSTULANTE.COD_ALUMNO        = MATRICUL.COD_ALUMNO AND
          POSTULANTE.COD_LINEA_NEGOCIO     = MATRICUL.COD_LINEA_NEGOCIO AND
          POSTULANTE.COD_MODAL_EST       = MATRICUL.COD_MODAL_EST AND
              MATRICUL.COD_LINEA_NEGOCIO      = v_cod_linea_negocio AND
          MATRICUL.COD_MODAL_EST       = v_cod_modal_est AND
          MATRICUL.COD_PERIOD_MAT       = v_cod_periodo AND
                             (SELECT count(m.cod_period_mat) FROM MATRICULA M
                              WHERE M.COD_ALUMNO = POSTULANTE.COD_ALUMNO
                              AND M.COD_LINEA_NEGOCIO = v_cod_linea_negocio
                              AND M.COD_MODAL_EST = v_cod_modal_est
                              AND M.COD_PERIOD_MAT <= v_cod_periodo) =1  AND
          --PADRE.COD_PERSONA NOT IN (SELECT COD_PERSONA FROM USUARIO WHERE COD_TIPO_USUARIO = 101) AND --CSC-00262669-00
          POSTULANTE.COD_ALUMNO NOT IN (SELECT COD_ALUMNO_HIJO FROM USUARIO_ALUMNO )
    union
    SELECT    POSTULANTE.COD_ALUMNO     ,
              POSTULANTE.COD_PERSONA    cod_persona_h,
              PERSONA.APELLIDO_PATERN       apellido_patern_h,
              PERSONA.APELLIDO_MATERN       apellido_matern_h,
              PERSONA.NOMBRES               nombres_h,
              PERSONA.SEXO                  sexo_h,--
            PARENT_MADRE.DETALLE_PARIENTE,
              MADRE.COD_PERSONA             ,
              MADRE.APELLIDO_PATERN         ,
              MADRE.APELLIDO_MATERN         ,
              MADRE.NOMBRES                 ,
          DECODE ( PERSONA.FECHA_NACIMIENT, NULL, '00000000', TO_CHAR(PERSONA.FECHA_NACIMIENT,'DDMMYYYY'))  PERSONA_FECHA_NACIMIENT,
              (SELECT DESCRIPCION FROM VIA WHERE COD_VIA = PERSONA.COD_VIA) VIA,--I
              PERSONA.DIRECCION               ,
              PERSONA.URBANIZACION            ,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO))) DEPARTAMENTO,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO)) PROVINCIA,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO) DISTRITO,
              MADRE.SEXO,
              DECODE(
              (
              SELECT COUNT(1)
              FROM PARENTESCO PP
              WHERE COD_PARIENTE = MADRE.COD_PERSONA
              AND DETALLE_PARIENTE IN ('1','2','3') --IN ('PADRE', 'MADRE','APODERADO')  -- CSC-00261922-00  JDELACRU
              and exists (SELECT A.COD_PERSONA FROM MATRICULA M, ALUMNO A
                          WHERE M.COD_LINEA_NEGOCIO = A.COD_LINEA_NEGOCIO
                          AND M.COD_ALUMNO = A.COD_ALUMNO
                          AND A.COD_PERSONA = PP.COD_PERSONA
                          AND M.COD_LINEA_NEGOCIO = v_cod_linea_negocio
                          AND M.COD_MODAL_EST = v_cod_modal_est
                          AND M.COD_PERIOD_MAT = v_cod_periodo
                          )
              ), 1,DECODE ( PERSONA.FECHA_NACIMIENT, NULL, '12345678', TO_CHAR(PERSONA.FECHA_NACIMIENT,'DDMMYYYY')) ,'12345678') PASSWORD
         FROM POSTULANTE POSTULANTE ,
              PERSONA PERSONA ,
              PERSONA MADRE ,
              PARENTESCO PARENT_MADRE ,
          MATRICULA  MATRICUL
        WHERE PERSONA.COD_PERSONA              = POSTULANTE.COD_PERSONA AND
              PARENT_MADRE.COD_PERSONA      = PERSONA.COD_PERSONA AND
        -- CSC-00261922-00  JDELACRU
              PARENT_MADRE.DETALLE_PARIENTE = '2' AND --'MADRE' AND
        -- CSC-00261922-00  JDELACRU
            MADRE.ESTADO_ACTIVO      ='SI' AND
              MADRE.COD_PERSONA             = PARENT_MADRE.COD_PARIENTE AND
          POSTULANTE.COD_ALUMNO        = MATRICUL.COD_ALUMNO AND
          POSTULANTE.COD_LINEA_NEGOCIO     = MATRICUL.COD_LINEA_NEGOCIO AND
          POSTULANTE.COD_MODAL_EST       = MATRICUL.COD_MODAL_EST AND
              MATRICUL.COD_LINEA_NEGOCIO      = v_cod_linea_negocio AND
          MATRICUL.COD_MODAL_EST       = v_cod_modal_est AND
          MATRICUL.COD_PERIOD_MAT       = v_cod_periodo AND
                             (SELECT count(m.cod_period_mat) FROM MATRICULA M
                              WHERE M.COD_ALUMNO = POSTULANTE.COD_ALUMNO
                              AND M.COD_LINEA_NEGOCIO = v_cod_linea_negocio
                              AND M.COD_MODAL_EST = v_cod_modal_est
                              AND M.COD_PERIOD_MAT <= v_cod_periodo) =1  AND
            --MADRE.COD_PERSONA NOT IN (SELECT COD_PERSONA FROM USUARIO WHERE COD_TIPO_USUARIO = 101) AND --CSC-00262669-00
            POSTULANTE.COD_ALUMNO NOT IN (SELECT COD_ALUMNO_HIJO FROM USUARIO_ALUMNO )
    UNION
    SELECT    POSTULANTE.COD_ALUMNO     ,
              POSTULANTE.COD_PERSONA    cod_persona_h,
              PERSONA.APELLIDO_PATERN       apellido_patern_h,
              PERSONA.APELLIDO_MATERN       apellido_matern_h,
              PERSONA.NOMBRES               nombres_h,
              PERSONA.SEXO                  sexo_h,
          PARENT_APODE.DETALLE_PARIENTE,
              APODERADO.COD_PERSONA         ,
              APODERADO.APELLIDO_PATERN     ,
              APODERADO.APELLIDO_MATERN     ,
              APODERADO.NOMBRES             ,
          DECODE ( PERSONA.FECHA_NACIMIENT, NULL, '00000000', TO_CHAR(PERSONA.FECHA_NACIMIENT,'DDMMYYYY'))  PERSONA_FECHA_NACIMIENT,
              (SELECT DESCRIPCION FROM VIA WHERE COD_VIA = PERSONA.COD_VIA) VIA,--I
              PERSONA.DIRECCION               ,
              PERSONA.URBANIZACION            ,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO))) DEPARTAMENTO,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO IN (SELECT COD_UBIGEO_PADR FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO)) PROVINCIA,
              (SELECT NOMBRE FROM UBIGEO WHERE COD_UBIGEO = PERSONA.COD_UBIGEO) DISTRITO,
              APODERADO.SEXO,
              DECODE(
              (
              SELECT COUNT(1)
              FROM PARENTESCO
              WHERE COD_PARIENTE = APODERADO.COD_PERSONA
              AND DETALLE_PARIENTE IN ('1','2','3') --IN ('PADRE', 'MADRE','APODERADO') -- CSC-00261922-00  JDELACRU
              ), 1,DECODE ( PERSONA.FECHA_NACIMIENT, NULL, '12345678', TO_CHAR(PERSONA.FECHA_NACIMIENT,'DDMMYYYY')) ,'12345678') PASSWORD
         FROM POSTULANTE POSTULANTE ,
              PERSONA PERSONA ,
              PERSONA APODERADO ,
              PARENTESCO PARENT_APODE ,
          MATRICULA  MATRICUL
        WHERE PERSONA.COD_PERSONA              = POSTULANTE.COD_PERSONA AND
              PARENT_APODE.COD_PERSONA         = PERSONA.COD_PERSONA AND
         -- CSC-00261922-00  JDELACRU
              PARENT_APODE.DETALLE_PARIENTE     = '3' AND --'APODERADO' AND
         -- CSC-00261922-00  JDELACRU
              APODERADO.COD_PERSONA         = PARENT_APODE.COD_PARIENTE AND
             APODERADO.ESTADO_ACTIVO      ='SI' AND
          POSTULANTE.COD_ALUMNO        = MATRICUL.COD_ALUMNO AND
          POSTULANTE.COD_LINEA_NEGOCIO     = MATRICUL.COD_LINEA_NEGOCIO AND
          POSTULANTE.COD_MODAL_EST       = MATRICUL.COD_MODAL_EST AND
              MATRICUL.COD_LINEA_NEGOCIO      = v_cod_linea_negocio AND
          MATRICUL.COD_MODAL_EST       = v_cod_modal_est AND
          MATRICUL.COD_PERIOD_MAT       = v_cod_periodo AND
                             (SELECT count(m.cod_period_mat) FROM MATRICULA M
                              WHERE M.COD_ALUMNO = POSTULANTE.COD_ALUMNO
                              AND M.COD_LINEA_NEGOCIO = v_cod_linea_negocio
                              AND M.COD_MODAL_EST = v_cod_modal_est
                              AND M.COD_PERIOD_MAT <= v_cod_periodo) =1  AND
            --APODERADO.COD_PERSONA NOT IN (SELECT COD_PERSONA FROM USUARIO WHERE COD_TIPO_USUARIO = 101) AND --CSC-00262669-00
            POSTULANTE.COD_ALUMNO NOT IN (SELECT COD_ALUMNO_HIJO FROM USUARIO_ALUMNO )
    )
    order by apellido_patern_H,apellido_matern_h,nombres_h, orden
) LOOP
begin
select count(1) into vv
from ges_solicitud_ppff
where cod_persona_h = x.COD_PERSONA_H
AND COD_LINEA_NEGOCIO      = v_cod_linea_negocio
AND COD_MODAL_EST       = v_cod_modal_est
AND COD_PERIODO       = v_cod_periodo;
exception when others then
vv:= 0;
end;
if vv= 0 then
begin
    westado_solicitud:=null;
    WPASSWORD:=null;
    WUSUARIO_ANT:=null;
    WBASEANTIGUA:=0;
   --Luego de insertar el registro se verifica si el ppff es único para la base en la temp
    select count(1)
    into WBASEANTIGUA
    from tmp_cuenta_ppff
    where cod_linea_negocio = v_cod_linea_negocio
    and cod_modalidad = v_cod_modal_est
    and cod_periodo = v_cod_periodo
    and cod_persona = x.COD_PERSONA; --Si el ppff ya existe
    --Se registra en una temporal los datos para verificar si los alumnos estan en la misma base
    begin
     insert into tmp_cuenta_ppff (COD_LINEA_NEGOCIO,COD_MODALIDAD,COD_PERIODO,COD_PERSONA,COD_PERSONA_H,DETALLE_PARIENTE,COD_ALUMNO)
     values (v_cod_linea_negocio,v_cod_modal_est,v_cod_periodo,x.COD_PERSONA ,x.COD_PERSONA_H,x.DETALLE_PARIENTE, X.cod_alumno );
     EXCEPTION WHEN OTHERS THEN
      NULL;
    end;
    --Se debe verificar si para algun registro anterior se proceso el mismo usuario
    --Caso: Hijo antiguo y 2 hijos en el periodo que se consulta
    select count(1) into valor_antiguo
    from ges_solicitud_ppff
    where cod_linea_negocio = v_cod_linea_negocio
    and cod_modal_est = v_cod_modal_est
    and cod_periodo <> v_cod_periodo
    and cod_persona = x.COD_PERSONA;
    if valor_antiguo > 0 then
     WBASEANTIGUA := 0;
    end if;
    PCOUNT := PCOUNT + 1;
    VUSUARIO := SF_OBTENER_USUARIO_PPFF(v_cod_linea_negocio,UPPER(X.APELLIDO_PATERN_H),UPPER(X.APELLIDO_MATERN_H),UPPER(X.NOMBRES_H));
    SELECT COUNT(1) INTO V
    FROM USUARIO
    WHERE COD_PERSONA = X.COD_PERSONA
    AND COD_TIPO_USUARIO = 101;
    IF V = 0 THEN
    WUSUARIO_ANT:=0;
      insert into usuario (COD_USUARIO,CLAVE_SECRETA,INTENTOS_FALLIDOS,ESTADO_USUARIO,COD_PERSONA,COD_TIPO_USUARIO,
      REQUIERE_PERMISO,FECHA_CREACION ,USUARIO_CREADOR,IND_ENC_EXA_ONLINE)
      VALUES (VUSUARIO,X.PASSWORD,0,'AC',X.cod_persona,101,'NO',sysdate,'ADMCUEPPFF','NO');
    ELSE
    WUSUARIO_ANT:=1;
      SELECT COD_USUARIO INTO VUSUARIO
      FROM USUARIO
      WHERE COD_PERSONA = X.COD_PERSONA
      AND COD_TIPO_USUARIO = 101
      AND ROWNUM = 1;
    END IF;
    BEGIN
    insert into usuario_alumno (COD_USUARIO_PADRE,COD_ALUMNO_HIJO,COD_LINEA_NEGOCIO_HIJO
    ,ESTADO,FECHA_CREACION,USUARIO_CREADOR)
    VALUES (VUSUARIO,X.cod_alumno,v_cod_linea_negocio,'AC',sysdate,'ADMCUEPPFF');
    EXCEPTION WHEN OTHERS THEN
      NULL;
    END;
    --CSC-00262669-00
    IF WUSUARIO_ANT = 1 and WBASEANTIGUA = 0 THEN --Si ya existia la cuenta no debe crearse nuevamente
    --Sólo debe ingresar aqui si el usuario fue creado en un periodo antiguo y no es
    --de la misma base que esta procesando
    --Se le agrega el estado = 3 para que lo considere en el excel para las cartas
      westado_solicitud := 3;
      WPASSWORD := 'ANTIGUO';
    ELSE
      westado_solicitud := 1;
      WPASSWORD := x.PASSWORD ;
    END IF;
    --FIN CSC-00262669-00
    insert into ges_solicitud_ppff
      (id_solicitud, fecha_solicitud, cod_linea_negocio, cod_modal_est, cod_periodo,
      observacion, estado_solicitud, usuario_creacion, usuario_modificacion, fecha_creacion, fecha_modificacion, ind_creo_cuentaad, password,
      cod_alumno, cod_persona_h, apellido_patern_h, apellido_matern_h, nombres_h, sexo_h,
      detalle_pariente, cod_persona, apellido_patern, apellido_matern, nombres, fecha_nacimiento,
      via, direccion, urbanizacion, departamento, provincia,distrito, sexo, cod_usuario)
    values(
      GES_SOL_PPFF_SEQ.Nextval, sysdate, v_cod_linea_negocio, v_cod_modal_est, v_cod_periodo,
          '', westado_solicitud,
          v_usuario_creacion, null, sysdate, null, 'NO',
        WPASSWORD ,
        x.COD_ALUMNO,
        x.COD_PERSONA_H,
        x.APELLIDO_PATERN_H,
        x.APELLIDO_MATERN_H,
        x.NOMBRES_H,
        x.SEXO_H,
        x.DETALLE_PARIENTE,
        x.COD_PERSONA,
        x.APELLIDO_PATERN,
        x.APELLIDO_MATERN,
        x.NOMBRES,
        x.PERSONA_FECHA_NACIMIENT,
        x.VIA,
        x.DIRECCION,
        x.URBANIZACION,
        x.DEPARTAMENTO,
        x.PROVINCIA,
        x.DISTRITO,
        x.SEXO,
        VUSUARIO
       );
		/* Control de Cambios
		Ticket : CSC-00263665-00
		Fecha:  29/08/2017
		Funcionalidad:  * Añadir rol para la Creacion de cuentas automaticas de padre de familia.
						* Se agrego la excepcion en el caso que exista el VUSUARIO ya no lo insertaria en la tabla USUARIO_ROL
		Programador: Felix Miranda Robles
		
		*/
    
     BEGIN
    
       INSERT INTO usuario_rol
	   (cod_rol, cod_usuario,fecha_inicio, fecha_fin, puede_delegar,
	   fecha_creacion, fecha_modificacion, usuario_modificador, usuario_creador)
	    VALUES ('2718',VUSUARIO,sysdate,TO_DATE('30/12/2090','DD/MM/RRRR'),'NO',sysdate,null, null,'ADMCUEPPFF'); 
	  
     EXCEPTION WHEN OTHERS THEN
      NULL;
    end;
	   --FIN CSC-00263665-00
end;
end if;
    END LOOP;
    PRESULTADO := '1';
    end if;
end;
PROCEDURE SP_OBTENER_SOL_CTAS_ADEXCH(Vperiodo periodo.cod_periodo%type,
                                     PC_OUT_CURSOR     OUT I_CURSOR,
                                     PC_OUT_RESULTADO  OUT VARCHAR2)
IS
BEGIN
   PC_OUT_RESULTADO := '';
   -- RECOGEMOS LAS SOLICITUDES DE CUENTAS PPFF
   OPEN PC_OUT_CURSOR  FOR
   select id_solicitud, cod_usuario , password, cod_persona, apellido_patern, apellido_matern, nombres--fecha_solicitud, cod_linea_negocio, cod_modal_est, cod_periodo, desc_grupo, observacion, estado_solicitud, usuario_creacion, usuario_modificacion, fecha_creacion, fecha_modificacion, ind_creo_cuentaad, cod_alumno, cod_persona_h, apellido_patern_h, apellido_matern_h, nombres_h, sexo_h, detalle_pariente, fecha_nacimiento, via, direccion, urbanizacion, departamento, provincia, distrito, sexo
   FROM Ges_Solicitud_PPFF GS
   WHERE GS.ESTADO_SOLICITUD IN ('1','2')
   and cod_usuario is not null
   AND cod_periodo = Vperiodo
   order by 1 desc;
EXCEPTION
  WHEN OTHERS THEN
       PC_OUT_RESULTADO:= 'Ha ocurrido un error al cargar las solicitudes: ' || sqlerrm;
END;
PROCEDURE SP_OBTENER_ARCHIVO_OK(Vperiodo periodo.cod_periodo%type,
                                PC_OUT_CURSOR     OUT I_CURSOR,
                                PC_OUT_RESULTADO  OUT VARCHAR2)
IS
BEGIN
   PC_OUT_RESULTADO := '';
   -- RECOGEMOS LAS SOLICITUDES DE CUENTAS PPFF
   OPEN PC_OUT_CURSOR  FOR
   /*SELECT 0,'COD_USUARIO','PASSWORD','COD_ALUMNO','APELLIDO_PATERN_H','APELLIDO_MATERN_H','NOMBRES_H','SEXO_H', 'APELLIDO_PATERN', 'APELLIDO_MATERN', 'NOMBRES','VIA','DIRECCION','URBANIZACION','DEPARTAMENTO','PROVINCIA','DISTRITO','SEXO' FROM DUAL
    UNION*/
    /* CONTROL DE CAMBIOS
    Responsable: MCAPCHA
    FECHA: 15/06/2016
    Ticket: CSC-00263073-00
    Funcionalidad: Añadir al proceso de generación de cartas de padres de familia los siguientes campos: Direccion_email, telefono_movil, telefono_casa, telefono_casa2.*/
    SELECT t1.COD_USUARIO,
    ''''||t1.PASSWORD PASSWORD,
    t1.COD_ALUMNO, 
    UPPER(t1.APELLIDO_PATERN_H) APELLIDO_PATERN_H, 
    UPPER(t1.APELLIDO_MATERN_H) APELLIDO_MATERN_H, 
    UPPER(t1.NOMBRES_H) NOMBRES_H, 
    t1.SEXO_H, 
    UPPER(t1.APELLIDO_PATERN) APELLIDO_PATERN_APO, 
    UPPER(t1.APELLIDO_MATERN) APELLIDO_MATERN_APO, 
    UPPER(t1.NOMBRES) NOMBRES_APO, 
    t1.SEXO SEXO_APO, 
    t1.VIA VIA_APO,
    t1.DIRECCION DIRECCION_APO,
    t1.URBANIZACION URBANIZACION_APO ,
    t1.DEPARTAMENTO DEPARTAMENTO_APO,
    t1.PROVINCIA PROVINCIA_APO,
    t1.DISTRITO DISTRITO_APO,
    t2.DIRECCION_EMAIL DIRECCION_EMAIL_APO,
    ''''||to_char(t2.TELEFONO_MOVIL) TELEFONO_MOVIL_APO,
    ''''||to_char(t2.TELEFONO_CASA) TELEFONO_CASA_APO,
    ''''||to_char(t2.TELEFONO_CASA2) TELEFONO_CASA2_APO
    FROM GES_SOLICITUD_PPFF t1
    LEFT OUTER JOIN PERSONA t2 
    ON t1.cod_persona = t2.cod_persona
    WHERE t1.ESTADO_SOLICITUD = 3 
    AND t1.cod_periodo = Vperiodo 
    order by t1.COD_USUARIO asc; 
    /*Fin  CSC-00263073-00*/
EXCEPTION
  WHEN OTHERS THEN
       PC_OUT_RESULTADO:= 'Ha ocurrido un error al cargar las solicitudes: ' || sqlerrm;
END;
FUNCTION SF_OBTENER_USUARIO_PPFF(V_COD_LINEA_NEGOCIO IN VARCHAR2,V_APE_PATERN IN VARCHAR2, V_APE_MATERN IN VARCHAR2, VNOMB IN VARCHAR2
  ) return varchar2
AS
V_APELLIDO_PATERN PERSONA.APELLIDO_PATERN%TYPE;
V_APELLIDO_MATERN PERSONA.APELLIDO_MATERN%TYPE;
VNOMBRES PERSONA.NOMBRES%TYPE;
VUSUARIO VARCHAR2(10);
VUSUARIO2 VARCHAR2(10);
VCONT NUMBER(2);
VCONT2 NUMBER(2);
VRESTO VARCHAR2(10);
BEGIN
    BEGIN
    /*
    UPPER(REPLACE(REPLACE(UPPER(APELLIDO_PATERN_H),'?','N'),' ','.')) APELLIDO_PATERN_H,
    UPPER(REPLACE(REPLACE(UPPER(APELLIDO_MATERN_H),'?','N'),' ','.')) APELLIDO_MATERN_H,
    UPPER(REPLACE(REPLACE(UPPER(NOMBRES_H),'?','N'),' ','.')) NOMBRES_H,
    */
    /*
    Se aumentará un REPLACE para que cuando se tenga una Ñ se reemplace por N antes de crear el usuario de Padre de Familia
    CSC-00262974-00 JMOGOLLO
    */
    V_APELLIDO_PATERN := REPLACE(REPLACE(REPLACE(UPPER(V_APE_PATERN),'?','N'),' ',''),'Ñ','N');
    V_APELLIDO_MATERN := REPLACE(REPLACE(REPLACE(UPPER(V_APE_MATERN),'?','N'),' ',''),'Ñ','N');
    VNOMBRES :=  REPLACE(REPLACE(REPLACE(UPPER(VNOMB),'?','N'),' ',''),'Ñ','N');
    --DBMS_OUTPUT.put_line(V_APE_PATERN);
    --DBMS_OUTPUT.put_line(V_APELLIDO_PATERN);
    IF V_APELLIDO_PATERN IS NULL OR VNOMBRES IS NULL THEN
      RETURN NULL;
    END IF;
    VUSUARIO :=  V_COD_LINEA_NEGOCIO || 'F';
    IF LENGTH(V_APELLIDO_PATERN) >= 3 THEN
    VUSUARIO := VUSUARIO || SUBSTR(V_APELLIDO_PATERN,1,3);
    ELSE
    VUSUARIO := VUSUARIO || V_APELLIDO_PATERN;
    END IF;
    IF LENGTH(V_APELLIDO_MATERN) >= 3 THEN
    VUSUARIO := VUSUARIO || SUBSTR(V_APELLIDO_MATERN,1,3);
    ELSE
    VUSUARIO := VUSUARIO || V_APELLIDO_MATERN;
    END IF;
    VCONT:=  LENGTH(VUSUARIO);
    VCONT2:= LENGTH(VNOMBRES);
    --DBMS_OUTPUT.put_line(VUSUARIO);
    IF VCONT < 8 THEN
       IF VCONT2 >= (8 - VCONT) THEN
       VUSUARIO := VUSUARIO || SUBSTR(VNOMBRES,1,(8 - VCONT));
       ELSE
         begin
         FOR I IN 1..(8 - VCONT - LENGTH(VNOMBRES)) LOOP
           VRESTO := VRESTO || '0';
         END LOOP;
         VUSUARIO := VUSUARIO ||VNOMBRES || VRESTO;
         end;
       END IF;
    END IF;
    --DBMS_OUTPUT.put_line(VUSUARIO);
    IF VCONT > 0 AND VCONT2> 0 AND LENGTH(V_APELLIDO_PATERN) > 0 AND LENGTH(VUSUARIO) < 8 THEN
         FOR I IN 1..(8 - LENGTH(VUSUARIO)) LOOP
           VRESTO := VRESTO || '0';
         END LOOP;
         VUSUARIO := VUSUARIO || VRESTO;
    END IF;
    --DBMS_OUTPUT.put_line(VUSUARIO);
    FOR I IN 0..99 LOOP
        --DBMS_OUTPUT.put_line(TO_CHAR(LENGTH(VUSUARIO)));
        --DBMS_OUTPUT.put_line(trim(TO_CHAR(I,'00')));
        VUSUARIO2 := VUSUARIO || trim(TO_CHAR(I,'00'));
        --DBMS_OUTPUT.put_line(VUSUARIO2);
        BEGIN
          SELECT COUNT(1) INTO VCONT
          FROM USUARIO
          WHERE COD_USUARIO = VUSUARIO2;
          EXCEPTION WHEN OTHERS THEN
            VCONT := 0;
        END;
        IF LENGTH(VUSUARIO2) = 10 AND VCONT = 0 THEN
           RETURN VUSUARIO2;
        END IF;
    END LOOP;
    RETURN NULL;
    EXCEPTION WHEN OTHERS THEN
        return null;
    END;
end;
PROCEDURE SP_ACTUALIZAR_SOLICITUD(
                                   PTIPO            IN NUMBER,
                                   PESTADO_SOLICITUD IN GES_SOLICITUD_ALUM.ESTADO_SOLICITUD%TYPE,
                                   POBSERVACION IN GES_SOLICITUD_ALUM.OBSERVACION%TYPE,
                                   PUSUARIO IN GES_SOLICITUD_ALUM.USUARIO_CREACION%TYPE,
                                   PIND_CREO_CUENTAAD IN GES_SOLICITUD_ALUM.IND_CREO_CUENTAAD%TYPE,
                                   PID_SOLICITUD    IN GES_SOLICITUD_ALUM.ID_SOLICITUD%TYPE,
                                   PC_OUT_RESULTADO OUT VARCHAR2)
IS
BEGIN
PC_OUT_RESULTADO := '';
IF PTIPO = 1 THEN  -- ESTADO
  UPDATE ges_solicitud_ppff
  SET
  ESTADO_SOLICITUD  = PESTADO_SOLICITUD, -- 1 REGISTRADO 2 ERROR 3 OK
  OBSERVACION = POBSERVACION,
  USUARIO_MODIFICACION = PUSUARIO,
  FECHA_MODIFICACION = SYSDATE
  WHERE ID_SOLICITUD = PID_SOLICITUD;
  commit;
  /*if PESTADO_SOLICITUD = 3 then
  begin
     BEGIN
     insert into usuario_alumno (COD_USUARIO_PADRE,COD_ALUMNO_HIJO,COD_LINEA_NEGOCIO_HIJO
     ,ESTADO,FECHA_CREACION,USUARIO_CREADOR)
     select s.cod_usuario,s.cod_alumno,s.cod_linea_negocio,'AC',to_date(sysdate,'DD/MM/YYYY'),PUSUARIO
     from ges_solicitud_ppff s
     where id_solicitud = PID_SOLICITUD;
     insert into usuario (COD_USUARIO,CLAVE_SECRETA,INTENTOS_FALLIDOS,ESTADO_USUARIO,COD_PERSONA,COD_TIPO_USUARIO,
     REQUIERE_PERMISO,FECHA_CREACION ,USUARIO_CREADOR,IND_ENC_EXA_ONLINE)
     select s.cod_usuario,s.password,0,'AC',s.cod_persona,101,'NO',to_date(sysdate,'DD/MM/YYYY'),PUSUARIO,'NO'
     from ges_solicitud_ppff s
     where id_solicitud = PID_SOLICITUD;
     EXCEPTION WHEN OTHERS THEN
       NULL;
     END;
     commit;
  end;
  end if;*/
END IF;
IF PTIPO = 2 THEN -- IND_CREO_CUENTAAD
  UPDATE GES_SOLICITUD_ALUM
  SET
  IND_CREO_CUENTAAD  = PIND_CREO_CUENTAAD,
  USUARIO_MODIFICACION = PUSUARIO,
  FECHA_MODIFICACION = SYSDATE
  WHERE ID_SOLICITUD = PID_SOLICITUD;
  commit;
END IF;
EXCEPTION
  WHEN OTHERS THEN
       PC_OUT_RESULTADO:= 'Ha ocurrido un error al actualizar la solicitud: ' || sqlerrm;
END;
end PQ_GEN_ADM_CUENTA_PPFF;
/
