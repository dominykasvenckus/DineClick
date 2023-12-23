"use client";

import Footer from "@/app/components/footer";
import RegisteredUserHeader from "@/app/components/registered-user-header";
import { getAccessToken, getDecodedAccessToken } from "@/app/jwt-utils";
import { Error, Reservation } from "@/app/types";
import { useRouter } from "next/navigation";
import { ChangeEvent, FormEvent, ReactNode, useEffect, useState } from "react";

export default function Create({
  params,
}: {
  params: { cityId: number; restaurantId: number };
}) {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [formData, setFormData] = useState({
    date: "",
    time: "",
    partySize: 2,
  });
  const [successMessage, setSuccessMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const router = useRouter();

  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const name = e.target.name;
    let value = e.target.value;
    if (name === "time") {
      value += ":00";
    }
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    setSuccessMessage("");
    setErrorMessage("");

    const response = await fetch(
      `/api/v1/cities/${params.cityId}/restaurants/${params.restaurantId}/reservations`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify(formData),
      }
    );

    if (response.ok) {
      const reservation: Reservation = await response.json();
      setFormData({
        date: reservation.date,
        time: reservation.time,
        partySize: reservation.partySize,
      });
      setSuccessMessage("Reservation created successfully");
    }

    if (response.status === 400) {
      setErrorMessage("Correctly fill in all the fields.");
    }

    if (response.status === 422) {
      const errors: Error[] = await response.json();
      const errorMessage = errors.map((error) => error.error).join("\n");
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
        case "RestaurantManager":
        case "Admin":
          router.push("/login");
          return;
        case "RegisteredUser":
          setContent(
            <>
              <RegisteredUserHeader />
              <div className="w-4/5 p-6 m-auto bg-white dark:bg-gray-700 rounded-md shadow-md ring-2 ring-gray-800/50 dark:ring-gray-300/50 md:max-w-lg my-16">
                <h1 className="text-xl font-semibold text-center text-gray-700 dark:text-white">
                  Reservation
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
                      htmlFor="date"
                      className="label text-sm md:text-base"
                    >
                      Date
                    </label>
                    <input
                      id="date"
                      type="date"
                      name="date"
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="time"
                      className="label text-sm md:text-base"
                    >
                      Time
                    </label>
                    <input
                      id="time"
                      type="time"
                      name="time"
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-2">
                    <label
                      htmlFor="partySize"
                      className="label text-sm md:text-base"
                    >
                      Party size
                    </label>
                    <input
                      id="partySize"
                      type="number"
                      name="partySize"
                      min={1}
                      defaultValue={formData.partySize}
                      onChange={handleChange}
                      className="w-full input input-bordered text-sm md:text-base"
                    />
                  </div>
                  <div className="mt-8">
                    <button type="submit" className="btn btn-block">
                      Create
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

  return content;
}
