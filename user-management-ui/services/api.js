import axios from "axios";

const API_URL = "https://localhost:7226/api";

export const getUsers = () => axios.get(`${API_URL}/users`);

export const createUser = (data) =>
  axios.post(`${API_URL}/users`, data);

export const updateUser = (id, data) =>
  axios.put(`${API_URL}/users/${id}`, data);

export const deleteUser = (id) =>
  axios.delete(`${API_URL}/users/${id}`);