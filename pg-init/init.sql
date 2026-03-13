--
-- PostgreSQL database dump
--

-- Dumped from database version 15.13 (Debian 15.13-1.pgdg120+1)
-- Dumped by pg_dump version 15.13 (Debian 15.13-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: citext; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public;


--
-- Name: EXTENSION citext; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION citext IS 'data type for case-insensitive character strings';


--
-- Name: pgcrypto; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;


--
-- Name: EXTENSION pgcrypto; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pgcrypto IS 'cryptographic functions';


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: audit_logs; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.audit_logs (
    id bigint NOT NULL,
    user_id bigint,
    action text NOT NULL,
    ip inet,
    user_agent text,
    data jsonb,
    logged_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.audit_logs OWNER TO auth_admin;

--
-- Name: audit_logs_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.audit_logs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.audit_logs_id_seq OWNER TO auth_admin;

--
-- Name: audit_logs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.audit_logs_id_seq OWNED BY public.audit_logs.id;


--
-- Name: login_attempts; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.login_attempts (
    id bigint NOT NULL,
    user_id bigint,
    email public.citext,
    succeeded boolean NOT NULL,
    ip inet,
    user_agent text,
    attempted_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.login_attempts OWNER TO auth_admin;

--
-- Name: login_attempts_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.login_attempts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.login_attempts_id_seq OWNER TO auth_admin;

--
-- Name: login_attempts_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.login_attempts_id_seq OWNED BY public.login_attempts.id;


--
-- Name: menus; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.menus (
    id bigint NOT NULL,
    parent_id bigint,
    path character varying(200) NOT NULL,
    label character varying(100) NOT NULL,
    icon_class character varying(64),
    sort_order integer DEFAULT 0 NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.menus OWNER TO auth_admin;

--
-- Name: menus_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.menus_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.menus_id_seq OWNER TO auth_admin;

--
-- Name: menus_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.menus_id_seq OWNED BY public.menus.id;


--
-- Name: role_menus; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.role_menus (
    role_id integer NOT NULL,
    menu_id bigint NOT NULL
);


ALTER TABLE public.role_menus OWNER TO auth_admin;

--
-- Name: roles; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.roles (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    home_menu_id bigint
);


ALTER TABLE public.roles OWNER TO auth_admin;

--
-- Name: roles_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.roles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.roles_id_seq OWNER TO auth_admin;

--
-- Name: roles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.roles_id_seq OWNED BY public.roles.id;


--
-- Name: tenants; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.tenants (
    id integer NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(100) NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL
);


ALTER TABLE public.tenants OWNER TO auth_admin;

--
-- Name: tenants_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.tenants_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tenants_id_seq OWNER TO auth_admin;

--
-- Name: tenants_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.tenants_id_seq OWNED BY public.tenants.id;


--
-- Name: tokens; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.tokens (
    id bigint NOT NULL,
    user_id bigint NOT NULL,
    token_hash character(64) NOT NULL,
    token_type character varying(32) NOT NULL,
    issued_at timestamp with time zone DEFAULT now() NOT NULL,
    expires_at timestamp with time zone NOT NULL,
    revoked_at timestamp with time zone,
    used boolean DEFAULT false NOT NULL
);


ALTER TABLE public.tokens OWNER TO auth_admin;

--
-- Name: tokens_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.tokens_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tokens_id_seq OWNER TO auth_admin;

--
-- Name: tokens_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.tokens_id_seq OWNED BY public.tokens.id;


--
-- Name: user_passwords; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.user_passwords (
    id bigint NOT NULL,
    user_id bigint NOT NULL,
    password_hash character varying NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    failed_attempts integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.user_passwords OWNER TO auth_admin;

--
-- Name: user_passwords_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.user_passwords_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_passwords_id_seq OWNER TO auth_admin;

--
-- Name: user_passwords_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.user_passwords_id_seq OWNED BY public.user_passwords.id;


--
-- Name: user_roles; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.user_roles (
    user_id bigint NOT NULL,
    role_id integer NOT NULL,
    is_primary boolean DEFAULT false NOT NULL
);


ALTER TABLE public.user_roles OWNER TO auth_admin;

--
-- Name: user_status; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.user_status (
    id integer NOT NULL,
    code character varying(32) NOT NULL,
    name character varying(64) NOT NULL
);


ALTER TABLE public.user_status OWNER TO auth_admin;

--
-- Name: user_status_history; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.user_status_history (
    id bigint NOT NULL,
    user_id bigint NOT NULL,
    status_id integer NOT NULL,
    started_at timestamp with time zone DEFAULT now() NOT NULL,
    ended_at timestamp with time zone,
    CONSTRAINT chk_end_after_start CHECK (((ended_at IS NULL) OR (ended_at > started_at)))
);


ALTER TABLE public.user_status_history OWNER TO auth_admin;

--
-- Name: user_status_history_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.user_status_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_status_history_id_seq OWNER TO auth_admin;

--
-- Name: user_status_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.user_status_history_id_seq OWNED BY public.user_status_history.id;


--
-- Name: user_status_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.user_status_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_status_id_seq OWNER TO auth_admin;

--
-- Name: user_status_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.user_status_id_seq OWNED BY public.user_status.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: auth_admin
--

CREATE TABLE public.users (
    id bigint NOT NULL,
    username character varying(64) NOT NULL,
    email public.citext NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    is_active boolean DEFAULT true NOT NULL,
    tenant_id integer NOT NULL,
    CONSTRAINT chk_username_len CHECK ((length((username)::text) >= 3))
);


ALTER TABLE public.users OWNER TO auth_admin;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: auth_admin
--

CREATE SEQUENCE public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.users_id_seq OWNER TO auth_admin;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: auth_admin
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: audit_logs id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.audit_logs ALTER COLUMN id SET DEFAULT nextval('public.audit_logs_id_seq'::regclass);


--
-- Name: login_attempts id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.login_attempts ALTER COLUMN id SET DEFAULT nextval('public.login_attempts_id_seq'::regclass);


--
-- Name: menus id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.menus ALTER COLUMN id SET DEFAULT nextval('public.menus_id_seq'::regclass);


--
-- Name: roles id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.roles ALTER COLUMN id SET DEFAULT nextval('public.roles_id_seq'::regclass);


--
-- Name: tenants id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tenants ALTER COLUMN id SET DEFAULT nextval('public.tenants_id_seq'::regclass);


--
-- Name: tokens id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tokens ALTER COLUMN id SET DEFAULT nextval('public.tokens_id_seq'::regclass);


--
-- Name: user_passwords id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_passwords ALTER COLUMN id SET DEFAULT nextval('public.user_passwords_id_seq'::regclass);


--
-- Name: user_status id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status ALTER COLUMN id SET DEFAULT nextval('public.user_status_id_seq'::regclass);


--
-- Name: user_status_history id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status_history ALTER COLUMN id SET DEFAULT nextval('public.user_status_history_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: audit_logs; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.audit_logs (id, user_id, action, ip, user_agent, data, logged_at) FROM stdin;
1	4	USER_REGISTERED	::1	PostmanRuntime/7.44.1	{"email": "prueba@gmail.com", "roleCode": "ADMIN", "username": "prueba"}	2026-03-07 19:37:57.246396+00
2	\N	LOGIN_FAILED	::1	PostmanRuntime/7.44.1	{"email": "prueba"}	2026-03-07 19:38:52.239658+00
3	\N	LOGIN_FAILED	::1	PostmanRuntime/7.44.1	{"email": "prueba"}	2026-03-07 19:39:34.744214+00
4	\N	LOGIN_FAILED	::1	PostmanRuntime/7.44.1	{"email": "prueba"}	2026-03-07 19:40:13.323391+00
5	\N	LOGIN_FAILED	::1	PostmanRuntime/7.44.1	{"email": "prueba"}	2026-03-07 19:41:35.11856+00
6	\N	LOGIN_FAILED	::1	PostmanRuntime/7.44.1	{"email": "prueba"}	2026-03-07 19:42:17.430271+00
7	4	LOGIN_SUCCESS	::1	PostmanRuntime/7.44.1	{"email": "prueba@gmail.com"}	2026-03-07 19:42:35.661498+00
8	4	LOGIN_SUCCESS	::1	PostmanRuntime/7.44.1	{"email": "prueba@gmail.com"}	2026-03-07 19:46:44.72949+00
9	4	LOGIN_SUCCESS	::1	PostmanRuntime/7.44.1	{"email": "prueba@gmail.com"}	2026-03-07 19:47:32.741699+00
10	\N	LOGOUT	::1	PostmanRuntime/7.44.1	{"success": true}	2026-03-07 19:50:34.502734+00
11	\N	LOGIN_FAILED	::1	PostmanRuntime/7.44.1	{"email": "prueba@gmail.com"}	2026-03-07 20:02:33.397457+00
12	\N	LOGIN_FAILED	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36	{"email": "andres"}	2026-03-07 21:34:24.61064+00
13	4	LOGIN_SUCCESS	::1	PostmanRuntime/7.44.1	{"email": "prueba@gmail.com"}	2026-03-07 21:39:08.487736+00
\.


--
-- Data for Name: login_attempts; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.login_attempts (id, user_id, email, succeeded, ip, user_agent, attempted_at) FROM stdin;
1	\N	prueba	f	::1	PostmanRuntime/7.44.1	2026-03-07 19:38:52.211994+00
2	\N	prueba	f	::1	PostmanRuntime/7.44.1	2026-03-07 19:39:34.739297+00
3	\N	prueba	f	::1	PostmanRuntime/7.44.1	2026-03-07 19:40:13.259258+00
4	\N	prueba	f	::1	PostmanRuntime/7.44.1	2026-03-07 19:41:23.03442+00
5	\N	prueba	f	::1	PostmanRuntime/7.44.1	2026-03-07 19:42:17.415006+00
6	4	prueba@gmail.com	t	::1	PostmanRuntime/7.44.1	2026-03-07 19:42:35.650287+00
7	4	prueba@gmail.com	t	::1	PostmanRuntime/7.44.1	2026-03-07 19:46:44.716918+00
8	4	prueba@gmail.com	t	::1	PostmanRuntime/7.44.1	2026-03-07 19:47:32.737303+00
9	4	prueba@gmail.com	f	::1	PostmanRuntime/7.44.1	2026-03-07 20:02:33.393593+00
10	\N	andres	f	::1	Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/145.0.0.0 Safari/537.36	2026-03-07 21:34:24.427279+00
11	4	prueba@gmail.com	t	::1	PostmanRuntime/7.44.1	2026-03-07 21:39:08.476054+00
\.


--
-- Data for Name: menus; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.menus (id, parent_id, path, label, icon_class, sort_order, is_active, created_at) FROM stdin;
1	\N	/admin	Administración	fa-cog	1	t	2025-06-16 01:06:03.741776+00
2	1	/admin/users	Usuarios	fa-users	1	t	2025-06-16 01:06:03.741776+00
\.


--
-- Data for Name: role_menus; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.role_menus (role_id, menu_id) FROM stdin;
1	1
1	2
\.


--
-- Data for Name: roles; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.roles (id, code, name, home_menu_id) FROM stdin;
1	ADMIN	Administrador	\N
2	USER	Usuario	\N
4	VISITA	Visita	\N
\.


--
-- Data for Name: tenants; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.tenants (id, code, name, is_active, created_at) FROM stdin;
1	default	Tenant por defecto	t	2026-03-07 21:29:22.114856+00
\.


--
-- Data for Name: tokens; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.tokens (id, user_id, token_hash, token_type, issued_at, expires_at, revoked_at, used) FROM stdin;
1	1	df356049a74d6a29840d801ea96ec7a97d385e6065ef0a211dfe9c98bba37ce4	refresh	2025-06-23 16:44:17.613639+00	2025-06-30 16:44:17.621633+00	\N	f
2	1	aa4bd7bfbce6daa00f0146ddc929d8aa729cc3fb22edf4a1604107de36649e5d	refresh	2025-06-23 17:16:19.778787+00	2025-06-30 17:16:19.77884+00	\N	f
3	1	effa648242007859aab0c70d04703d44a78a97f65662d0d7e4e77d961d1c8334	refresh	2025-06-23 21:13:05.743322+00	2025-06-30 21:13:05.743378+00	\N	f
4	1	8d0059d4162abc1f657c1bb88b832fd1cb48949985a3f5015e6787a8b261c244	refresh	2025-06-23 21:16:08.156262+00	2025-06-30 21:16:08.167523+00	\N	f
5	1	eb093e386d9cac175dd1e73a2b776edaa0485312ee6bcfdec04f0b18d11643a0	refresh	2025-06-23 21:19:48.219206+00	2025-06-30 21:19:48.21925+00	\N	f
6	1	a6aa80b10511d56ff914efac7a930e5526e49f9098aeb4154f61bbdddaed0a27	refresh	2025-06-23 21:20:41.504278+00	2025-06-30 21:20:41.504319+00	2025-06-23 21:21:31.182987+00	t
7	1	d9ae41d162466208ea33f3395eb16720c633fef8f5ff312a71a4d00a6a796ab6	refresh	2025-06-23 21:22:05.995622+00	2025-06-30 21:22:06.004584+00	\N	f
8	4	498aa59c13c9f00a07d6a1d53f5cfe910863d6c3731613470dc597ecbd0a90a1	refresh	2026-03-07 19:42:34.250143+00	2026-03-14 19:42:34.250384+00	\N	f
9	4	a918c0b10d0958a93234c12ccc79502c9d672277436a4494d7c806527dea07e2	refresh	2026-03-07 19:46:42.313717+00	2026-03-14 19:46:42.313717+00	\N	f
10	4	039d61a3b8d8def2193b97b1877a5e481c0753756aebd3e13725c0a099b2ff17	refresh	2026-03-07 19:47:32.730961+00	2026-03-14 19:47:32.730961+00	2026-03-07 19:50:34.477809+00	t
11	4	80c11943a1119391faa3027a1038ae851fdc03a1f8d66759deb0556e8f06e0a4	refresh	2026-03-07 21:39:08.439993+00	2026-03-14 21:39:08.440041+00	\N	f
\.


--
-- Data for Name: user_passwords; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.user_passwords (id, user_id, password_hash, created_at, failed_attempts) FROM stdin;
3	1	123456	2025-06-18 02:56:05.067825+00	0
6	4	$2a$12$QEgrQ/w8HRXalfrb/pIl/e.f4QU2P4eHqe7iRZHTCxp6zw0XcGxH.	2026-03-07 19:37:57.137422+00	0
\.


--
-- Data for Name: user_roles; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.user_roles (user_id, role_id, is_primary) FROM stdin;
1	1	t
4	1	t
\.


--
-- Data for Name: user_status; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.user_status (id, code, name) FROM stdin;
1	NEW	Nuevo
2	ACTIVE	Activo
3	BLOCKED	Bloqueado
4	DELETED	Eliminado
5	INACTIVE	Inactivado
\.


--
-- Data for Name: user_status_history; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.user_status_history (id, user_id, status_id, started_at, ended_at) FROM stdin;
1	1	1	2025-06-18 04:34:26.540753+00	\N
2	1	2	2025-06-18 04:41:58.374756+00	\N
3	4	2	2026-03-07 19:37:57.175964+00	\N
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: auth_admin
--

COPY public.users (id, username, email, created_at, is_active, tenant_id) FROM stdin;
1	acomte	acomte@gmail.com	2025-06-18 01:37:11.611598+00	t	1
4	prueba	prueba@gmail.com	2026-03-07 19:37:57.097626+00	t	1
\.


--
-- Name: audit_logs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.audit_logs_id_seq', 13, true);


--
-- Name: login_attempts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.login_attempts_id_seq', 11, true);


--
-- Name: menus_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.menus_id_seq', 2, true);


--
-- Name: roles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.roles_id_seq', 4, true);


--
-- Name: tenants_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.tenants_id_seq', 1, true);


--
-- Name: tokens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.tokens_id_seq', 11, true);


--
-- Name: user_passwords_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.user_passwords_id_seq', 6, true);


--
-- Name: user_status_history_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.user_status_history_id_seq', 3, true);


--
-- Name: user_status_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.user_status_id_seq', 7, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: auth_admin
--

SELECT pg_catalog.setval('public.users_id_seq', 4, true);


--
-- Name: audit_logs audit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_pkey PRIMARY KEY (id);


--
-- Name: login_attempts login_attempts_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.login_attempts
    ADD CONSTRAINT login_attempts_pkey PRIMARY KEY (id);


--
-- Name: menus menus_parent_id_label_key; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.menus
    ADD CONSTRAINT menus_parent_id_label_key UNIQUE (parent_id, label);


--
-- Name: menus menus_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.menus
    ADD CONSTRAINT menus_pkey PRIMARY KEY (id);


--
-- Name: role_menus role_menus_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.role_menus
    ADD CONSTRAINT role_menus_pkey PRIMARY KEY (role_id, menu_id);


--
-- Name: roles roles_code_key; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_code_key UNIQUE (code);


--
-- Name: roles roles_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_pkey PRIMARY KEY (id);


--
-- Name: tenants tenants_code_key; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT tenants_code_key UNIQUE (code);


--
-- Name: tenants tenants_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tenants
    ADD CONSTRAINT tenants_pkey PRIMARY KEY (id);


--
-- Name: tokens tokens_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tokens
    ADD CONSTRAINT tokens_pkey PRIMARY KEY (id);


--
-- Name: tokens tokens_token_hash_key; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tokens
    ADD CONSTRAINT tokens_token_hash_key UNIQUE (token_hash);


--
-- Name: user_passwords user_passwords_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_passwords
    ADD CONSTRAINT user_passwords_pkey PRIMARY KEY (id);


--
-- Name: user_passwords user_passwords_user_id_key; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_passwords
    ADD CONSTRAINT user_passwords_user_id_key UNIQUE (user_id);


--
-- Name: user_roles user_roles_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT user_roles_pkey PRIMARY KEY (user_id, role_id);


--
-- Name: user_status user_status_code_key; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status
    ADD CONSTRAINT user_status_code_key UNIQUE (code);


--
-- Name: user_status_history user_status_history_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status_history
    ADD CONSTRAINT user_status_history_pkey PRIMARY KEY (id);


--
-- Name: user_status user_status_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status
    ADD CONSTRAINT user_status_pkey PRIMARY KEY (id);


--
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (id);


--
-- Name: idx_audit_logs_action_time; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX idx_audit_logs_action_time ON public.audit_logs USING btree (action, logged_at DESC);


--
-- Name: idx_audit_logs_user_time; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX idx_audit_logs_user_time ON public.audit_logs USING btree (user_id, logged_at DESC);


--
-- Name: idx_login_attempts_ip_time; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX idx_login_attempts_ip_time ON public.login_attempts USING btree (ip, attempted_at DESC);


--
-- Name: idx_login_attempts_user_time; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX idx_login_attempts_user_time ON public.login_attempts USING btree (user_id, attempted_at DESC);


--
-- Name: idx_menus_parent_sort; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX idx_menus_parent_sort ON public.menus USING btree (parent_id, sort_order);


--
-- Name: idx_tokens_user_expires; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX idx_tokens_user_expires ON public.tokens USING btree (user_id, expires_at);


--
-- Name: user_status_history_user_id_started_at_idx; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE INDEX user_status_history_user_id_started_at_idx ON public.user_status_history USING btree (user_id, started_at DESC);


--
-- Name: users_tenant_email_key; Type: INDEX; Schema: public; Owner: auth_admin
--

CREATE UNIQUE INDEX users_tenant_email_key ON public.users USING btree (tenant_id, email);


--
-- Name: audit_logs audit_logs_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.audit_logs
    ADD CONSTRAINT audit_logs_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: login_attempts login_attempts_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.login_attempts
    ADD CONSTRAINT login_attempts_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: menus menus_parent_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.menus
    ADD CONSTRAINT menus_parent_id_fkey FOREIGN KEY (parent_id) REFERENCES public.menus(id) ON DELETE CASCADE;


--
-- Name: role_menus role_menus_menu_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.role_menus
    ADD CONSTRAINT role_menus_menu_id_fkey FOREIGN KEY (menu_id) REFERENCES public.menus(id) ON DELETE CASCADE;


--
-- Name: role_menus role_menus_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.role_menus
    ADD CONSTRAINT role_menus_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: roles roles_home_menu_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.roles
    ADD CONSTRAINT roles_home_menu_id_fkey FOREIGN KEY (home_menu_id) REFERENCES public.menus(id);


--
-- Name: tokens tokens_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.tokens
    ADD CONSTRAINT tokens_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_passwords user_passwords_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_passwords
    ADD CONSTRAINT user_passwords_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_roles user_roles_role_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT user_roles_role_id_fkey FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE;


--
-- Name: user_roles user_roles_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT user_roles_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: user_status_history user_status_history_status_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status_history
    ADD CONSTRAINT user_status_history_status_id_fkey FOREIGN KEY (status_id) REFERENCES public.user_status(id);


--
-- Name: user_status_history user_status_history_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.user_status_history
    ADD CONSTRAINT user_status_history_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE;


--
-- Name: users users_tenant_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: auth_admin
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_tenant_id_fkey FOREIGN KEY (tenant_id) REFERENCES public.tenants(id);


--
-- PostgreSQL database dump complete
--

