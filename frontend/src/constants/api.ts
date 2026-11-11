import { pathBase } from "@/config/resqueue";

export const API_URL = import.meta.env.VITE_API_URL
  ? import.meta.env.VITE_API_URL
  : `${pathBase}${import.meta.env.BASE_URL}/api`;
