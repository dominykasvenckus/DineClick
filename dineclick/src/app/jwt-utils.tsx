import { jwtDecode } from "jwt-decode";
import { CustomJwtPayload } from "./types";

export const setAccessToken = (accessToken: string) => {
  localStorage.setItem("access_token", accessToken);
};

const refreshAccessToken = async (refreshToken: string) => {
  const data = { refreshToken: refreshToken };
  const response = await fetch("/api/v1/accessToken", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  if (response.ok) {
    const responseData: { accessToken: string; refreshToken: string } =
      await response.json();
    setAccessToken(responseData.accessToken);
    setRefreshToken(responseData.refreshToken);
  }
};

export const getAccessToken = async () => {
  let accessToken = localStorage.getItem("access_token");
  if (!accessToken) {
    const refreshToken = localStorage.getItem("refresh_token");
    if (refreshToken) {
      await refreshAccessToken(refreshToken);
      accessToken = localStorage.getItem("access_token");
    }
  }
  return accessToken;
};

export const getDecodedAccessToken = async () => {
  let accessToken = localStorage.getItem("access_token");
  if (!accessToken) {
    const refreshToken = localStorage.getItem("refresh_token");
    if (refreshToken) {
      await refreshAccessToken(refreshToken);
      accessToken = localStorage.getItem("access_token");
    }
  }
  return accessToken ? jwtDecode<CustomJwtPayload>(accessToken) : null;
};

export const removeAccessToken = () => {
  localStorage.removeItem("access_token");
};

export const setRefreshToken = (refreshToken: string) => {
  localStorage.setItem("refresh_token", refreshToken);
};

export const removeRefreshToken = () => {
  localStorage.removeItem("refresh_token");
};
