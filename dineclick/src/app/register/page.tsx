"use client";

import { useRouter } from "next/navigation";
import { ChangeEvent, FormEvent, useState } from "react";
import { Error } from "../types";

export default function Register() {
  const [formData, setFormData] = useState({
    username: "",
    password: "",
    email: "",
    firstName: "",
    lastName: "",
    role: 0,
  });
  const [errorMessage, setErrorMessage] = useState("");
  const router = useRouter();

  const handleChange = (
    e: ChangeEvent<HTMLInputElement> | ChangeEvent<HTMLSelectElement>
  ) => {
    const name = e.target.name;
    let value: string | number = e.target.value;
    if (name === "role") {
      value = Number(value);
    }
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const response = await fetch("/api/v1/register", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(formData),
    });

    if (response.status === 201) {
      router.push("/login");
    }

    if (response.status === 422) {
      const errorData: Error[] = await response.json();
      const errorMessage = errorData.map((error) => error.error).join("\n");
      setErrorMessage(errorMessage);
    }
  };

  return (
    <div className="relative flex flex-col justify-center min-h-screen overflow-hidden">
      <div className="w-4/5 p-6 m-auto bg-white dark:bg-gray-700 rounded-md shadow-md ring-2 ring-gray-800/50 dark:ring-gray-300/50 md:max-w-lg my-8">
        <h1 className="text-2xl md:text-3xl font-semibold text-center text-gray-700 dark:text-white">
          DineClick
        </h1>
        {errorMessage && (
          <div className="text-red-500 text-sm md:text-base mt-2 whitespace-pre-line">
            {errorMessage}
          </div>
        )}
        <form method="post" onSubmit={handleSubmit} autoComplete="off">
          <div>
            <label htmlFor="username" className="label text-sm md:text-base">
              Username
            </label>
            <input
              id="username"
              type="text"
              name="username"
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
              placeholder="Enter password"
              onChange={handleChange}
              className="w-full input input-bordered text-sm md:text-base"
            />
          </div>
          <div className="mt-2">
            <label htmlFor="email" className="label text-sm md:text-base">
              Email
            </label>
            <input
              id="email"
              type="email"
              name="email"
              placeholder="Enter email"
              onChange={handleChange}
              className="w-full input input-bordered text-sm md:text-base"
            />
          </div>
          <div className="mt-2">
            <label htmlFor="firstName" className="label text-sm md:text-base">
              First name
            </label>
            <input
              id="firstName"
              type="text"
              name="firstName"
              placeholder="Enter first name"
              onChange={handleChange}
              className="w-full input input-bordered text-sm md:text-base"
            />
          </div>
          <div className="mt-2">
            <label htmlFor="lastName" className="label text-sm md:text-base">
              Last name
            </label>
            <input
              id="lastName"
              type="text"
              name="lastName"
              placeholder="Enter last name"
              onChange={handleChange}
              className="w-full input input-bordered text-sm md:text-base"
            />
          </div>
          <div className="mt-2">
            <label htmlFor="role" className="label text-sm md:text-base">
              Role
            </label>
            <select
              id="role"
              name="role"
              className="w-full select select-bordered text-sm md:text-base"
              defaultValue={0}
              onChange={handleChange}
            >
              <option value={0}>Registered user</option>
              <option value={1}>Restaurant manager</option>
            </select>
          </div>
          <div className="mt-8">
            <button type="submit" className="btn btn-block">
              Register
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
