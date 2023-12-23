"use client";

import { useRouter } from "next/navigation";
import { ChangeEvent, FormEvent, ReactNode, useEffect, useState } from "react";
import RegisteredUserHeader from "../components/registered-user-header";
import RestaurantManagerHeader from "../components/restaurant-manager-header";
import AdminHeader from "../components/admin-header";
import { getAccessToken, getDecodedAccessToken } from "../jwt-utils";
import { Error, User } from "../types";
import Footer from "../components/footer";

export default function Profile() {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [formData, setFormData] = useState({
    username: "",
    email: "",
    firstName: "",
    lastName: "",
  });
  const [successMessage, setSuccessMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const router = useRouter();

  const renderHeader = (role: string) => {
    switch (role) {
      case "RegisteredUser":
        return <RegisteredUserHeader />;
      case "RestaurantManager":
        return <RestaurantManagerHeader />;
      case "Admin":
        return <AdminHeader />;
      default:
        return null;
    }
  };

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const accessToken = await getAccessToken();
    const decodedAccessToken = await getDecodedAccessToken();
    if (!accessToken || !decodedAccessToken) {
      router.push("/login");
      return;
    }

    setSuccessMessage("");
    setErrorMessage("");

    const response = await fetch(`/api/v1/users/${decodedAccessToken.sub}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify(formData),
    });

    if (response.ok) {
      const user: User = await response.json();
      setFormData({
        username: user.username,
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
      });
      setSuccessMessage("Changes saved successfully.");
    }

    if (response.status === 422) {
      const errorData: Error[] = await response.json();
      const errorMessage = errorData.map((error) => error.error).join("\n");
      setErrorMessage(errorMessage);
    }
  };

  useEffect(() => {
    (async () => {
      let decodedAccessToken = await getDecodedAccessToken();
      if (!decodedAccessToken) {
        router.push("/login");
        return;
      }

      const role =
        decodedAccessToken[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ];

      switch (role) {
        case "RegisteredUser":
        case "RestaurantManager":
        case "Admin":
          setContent(
            <>
              {renderHeader(role)}
              <div className="w-4/5 p-6 m-auto bg-white dark:bg-gray-700 rounded-md shadow-md ring-2 ring-gray-800/50 dark:ring-gray-300/50 md:max-w-lg my-16">
                <h1 className="text-xl font-semibold text-center text-gray-700 dark:text-white">
                  Profile
                </h1>
                {successMessage && (
                  <div className="text-green-600 text-sm md:text-base mt-2">
                    {successMessage}
                  </div>
                )}
                {errorMessage && (
                  <div className="text-red-500 text-sm md:text-base mt-2 whitespace-pre-line">
                    {errorMessage}
                  </div>
                )}
                <form method="post" onSubmit={handleSubmit} autoComplete="off">
                  <div>
                    <label
                      htmlFor="username"
                      className="label text-sm md:text-base"
                    >
                      Username
                    </label>
                    <input
                      id="username"
                      type="text"
                      name="username"
                      placeholder="Enter username"
                      defaultValue={formData.username}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="email"
                      className="label text-sm md:text-base"
                    >
                      Email
                    </label>
                    <input
                      id="email"
                      type="text"
                      name="email"
                      placeholder="Enter email"
                      defaultValue={formData.email}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="firstName"
                      className="label text-sm md:text-base"
                    >
                      First name
                    </label>
                    <input
                      id="firstName"
                      type="text"
                      name="firstName"
                      placeholder="Enter first name"
                      defaultValue={formData.firstName}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="lastName"
                      className="label text-sm md:text-base"
                    >
                      Last name
                    </label>
                    <input
                      id="lastName"
                      type="text"
                      name="lastName"
                      placeholder="Enter last name"
                      defaultValue={formData.lastName}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-8">
                    <button type="submit" className="btn btn-block">
                      Save
                    </button>
                  </div>
                </form>
              </div>
              <Footer />
            </>
          );
          break;
      }
    })();
  }, [formData, successMessage, errorMessage]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      const decodedAccessToken = await getDecodedAccessToken();
      if (!accessToken || !decodedAccessToken) {
        router.push("/login");
        return;
      }

      const response = await fetch(`/api/v1/users/${decodedAccessToken.sub}`, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.ok) {
        const user: User = await response.json();
        setFormData({
          username: user.username,
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
        });
      }
    })();
  }, []);

  return content;
}
