--
-- PostgreSQL database dump
--

-- Dumped from database version 17.5
-- Dumped by pg_dump version 17.5

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: calendar; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.calendar (
    id integer NOT NULL,
    users_id integer NOT NULL,
    notification_date timestamp with time zone,
    date timestamp with time zone NOT NULL,
    title text NOT NULL,
    descriptions text,
    seen boolean DEFAULT true
);


ALTER TABLE public.calendar OWNER TO postgres;

--
-- Name: calendar_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.calendar_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.calendar_id_seq OWNER TO postgres;

--
-- Name: calendar_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.calendar_id_seq OWNED BY public.calendar.id;


--
-- Name: category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.category (
    id integer NOT NULL,
    users_id integer NOT NULL,
    main_parent integer NOT NULL,
    parent integer,
    name text NOT NULL,
    created timestamp with time zone,
    seen boolean DEFAULT true
);


ALTER TABLE public.category OWNER TO postgres;

--
-- Name: category_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.category_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.category_id_seq OWNER TO postgres;

--
-- Name: category_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.category_id_seq OWNED BY public.category.id;


--
-- Name: documents; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.documents (
    id integer NOT NULL,
    users_id integer NOT NULL,
    main_parent integer NOT NULL,
    parent integer,
    title text NOT NULL,
    text text,
    created_at timestamp with time zone,
    seen boolean DEFAULT true
);


ALTER TABLE public.documents OWNER TO postgres;

--
-- Name: documents_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.documents_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.documents_id_seq OWNER TO postgres;

--
-- Name: documents_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.documents_id_seq OWNED BY public.documents.id;


--
-- Name: main_category; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.main_category (
    id integer NOT NULL,
    users_id integer NOT NULL,
    name text NOT NULL,
    seen boolean DEFAULT true
);


ALTER TABLE public.main_category OWNER TO postgres;

--
-- Name: main_category_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.main_category_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.main_category_id_seq OWNER TO postgres;

--
-- Name: main_category_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.main_category_id_seq OWNED BY public.main_category.id;


--
-- Name: todo; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.todo (
    id integer NOT NULL,
    users_id integer NOT NULL,
    date timestamp with time zone NOT NULL,
    name text NOT NULL,
    status boolean DEFAULT false,
    seen boolean DEFAULT true
);


ALTER TABLE public.todo OWNER TO postgres;

--
-- Name: todo_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.todo_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.todo_id_seq OWNER TO postgres;

--
-- Name: todo_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.todo_id_seq OWNED BY public.todo.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id integer NOT NULL,
    email text NOT NULL,
    username text NOT NULL,
    password text NOT NULL,
    salt text NOT NULL,
    token_version integer DEFAULT 0,
    active boolean DEFAULT false
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_id_seq OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: wordstudy_flashcard; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.wordstudy_flashcard (
    id integer NOT NULL,
    users_id integer NOT NULL,
    sett_id integer NOT NULL,
    front text NOT NULL,
    back text NOT NULL,
    seen boolean DEFAULT true
);


ALTER TABLE public.wordstudy_flashcard OWNER TO postgres;

--
-- Name: wordstudy_flashcard_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.wordstudy_flashcard_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.wordstudy_flashcard_id_seq OWNER TO postgres;

--
-- Name: wordstudy_flashcard_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.wordstudy_flashcard_id_seq OWNED BY public.wordstudy_flashcard.id;


--
-- Name: wordstudy_folder; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.wordstudy_folder (
    id integer NOT NULL,
    users_id integer NOT NULL,
    name text NOT NULL,
    notification_date timestamp with time zone,
    end_date timestamp with time zone,
    seen boolean DEFAULT true,
    created timestamp with time zone DEFAULT now()
);


ALTER TABLE public.wordstudy_folder OWNER TO postgres;

--
-- Name: wordstudy_folder_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.wordstudy_folder_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.wordstudy_folder_id_seq OWNER TO postgres;

--
-- Name: wordstudy_folder_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.wordstudy_folder_id_seq OWNED BY public.wordstudy_folder.id;


--
-- Name: wordstudy_sett; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.wordstudy_sett (
    id integer NOT NULL,
    users_id integer NOT NULL,
    name text NOT NULL,
    notification_date timestamp with time zone,
    end_date timestamp with time zone,
    seen boolean DEFAULT true,
    folder_id integer,
    created timestamp with time zone DEFAULT now()
);


ALTER TABLE public.wordstudy_sett OWNER TO postgres;

--
-- Name: wordstudy_sett_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.wordstudy_sett_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.wordstudy_sett_id_seq OWNER TO postgres;

--
-- Name: wordstudy_sett_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.wordstudy_sett_id_seq OWNED BY public.wordstudy_sett.id;


--
-- Name: calendar id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.calendar ALTER COLUMN id SET DEFAULT nextval('public.calendar_id_seq'::regclass);


--
-- Name: category id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.category ALTER COLUMN id SET DEFAULT nextval('public.category_id_seq'::regclass);


--
-- Name: documents id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.documents ALTER COLUMN id SET DEFAULT nextval('public.documents_id_seq'::regclass);


--
-- Name: main_category id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.main_category ALTER COLUMN id SET DEFAULT nextval('public.main_category_id_seq'::regclass);


--
-- Name: todo id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.todo ALTER COLUMN id SET DEFAULT nextval('public.todo_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Name: wordstudy_flashcard id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wordstudy_flashcard ALTER COLUMN id SET DEFAULT nextval('public.wordstudy_flashcard_id_seq'::regclass);


--
-- Name: wordstudy_folder id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wordstudy_folder ALTER COLUMN id SET DEFAULT nextval('public.wordstudy_folder_id_seq'::regclass);


--
-- Name: wordstudy_sett id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wordstudy_sett ALTER COLUMN id SET DEFAULT nextval('public.wordstudy_sett_id_seq'::regclass);


--
-- Name: calendar calendar_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.calendar
    ADD CONSTRAINT calendar_pkey PRIMARY KEY (id);


--
-- Name: category category_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.category
    ADD CONSTRAINT category_pkey PRIMARY KEY (id);


--
-- Name: documents documents_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.documents
    ADD CONSTRAINT documents_pkey PRIMARY KEY (id);


--
-- Name: main_category main_category_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.main_category
    ADD CONSTRAINT main_category_pkey PRIMARY KEY (id);


--
-- Name: todo todo_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.todo
    ADD CONSTRAINT todo_pkey PRIMARY KEY (id);


--
-- Name: wordstudy_flashcard wordstudy_flashcard_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wordstudy_flashcard
    ADD CONSTRAINT wordstudy_flashcard_pkey PRIMARY KEY (id);


--
-- Name: wordstudy_folder wordstudy_folder_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.wordstudy_folder
    ADD CONSTRAINT wordstudy_folder_pkey PRIMARY KEY (id);


--
-- PostgreSQL database dump complete
--

