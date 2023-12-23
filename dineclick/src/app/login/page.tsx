"use client";

import { useRouter } from "next/navigation";
import { ChangeEvent, FormEvent, useState } from "react";
import {
  getDecodedAccessToken,
  setAccessToken,
  setRefreshToken,
} from "../jwt-utils";
import { Error } from "../types";

export default function Login() {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
  });
  const [errorMessage, setErrorMessage] = useState("");
  const router = useRouter();

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const response = await fetch("/api/v1/login", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(formData),
    });

    if (response.ok) {
      const tokens: { accessToken: string; refreshToken: string } =
        await response.json();
      setAccessToken(tokens.accessToken);
      setRefreshToken(tokens.refreshToken);

      const decodedAccessToken = await getDecodedAccessToken();
      const role =
        decodedAccessToken?.[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ];

      switch (role) {
        case "RegisteredUser":
        case "RestaurantManager":
          router.push("/");
          return;
        case "Admin":
          router.push("/");
          return;
      }
    }

    if (response.status === 403) {
      setErrorMessage("Your account is currently banned.");
    }

    if (response.status === 422) {
      const error: Error = await response.json();
      setErrorMessage(error.error);
    }
  };

  return (
    <div className="relative flex flex-col justify-center h-screen overflow-hidden">
      <div className="w-4/5 p-6 m-auto bg-white dark:bg-gray-700 rounded-md shadow-md ring-2 ring-gray-800/50 dark:ring-gray-300/50 md:max-w-lg">
        <h1 className="text-2xl md:text-3xl font-semibold text-center text-gray-700 dark:text-white">
          DineClick
        </h1>
        {errorMessage && (
          <div className="text-red-500 text-sm md:text-base mt-2">
            {errorMessage}
          </div>
        )}
        <form method="post" onSubmit={handleSubmit}>
          <div>
            <label htmlFor="username" className="label text-sm md:text-base">
              Username
            </label>
            <input
              id="username"
              type="text"
              name="username"
              autoComplete="username"
              placeholder="Enter username"
              onChange={handleChange}
              className="w-full input input-bordered text-sm md:text-base"
            />
          </div>
          <div className="mt-2">
            <label htmlFor="password" className="label text-sm md:text-base">
              Password
            </label>
            <input
              id="password"
              type="password"
              name="password"
              autoComplete="current-password"
              placeholder="Enter password"
              onChange={handleChange}
              className="w-full input input-bordered text-sm md:text-base"
            />
          </div>
          <div className="mt-8">
            <button type="submit" className="btn btn-block">
              Login
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
