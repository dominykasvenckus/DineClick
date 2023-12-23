"use client";

import { ReactNode, useEffect, useState } from "react";
import { City, Reservation, Restaurant } from "../types";
import { useRouter } from "next/navigation";
import { getAccessToken, getDecodedAccessToken } from "../jwt-utils";
import RegisteredUserHeader from "../components/registered-user-header";
import Footer from "../components/footer";

export default function Reservations() {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [cities, setCities] = useState<null | City[]>(null);
  const [selectedCityId, setSelectedCityId] = useState("");
  const [restaurants, setRestaurants] = useState<null | Restaurant[]>(null);
  const [reservations, setReservations] = useState<null | Reservation[]>(null);
  const router = useRouter();

  const getStatusName = (statusNumber: number) => {
    switch (statusNumber) {
      case 0:
        return "Pending";
      case 1:
        return "Confirmed";
      case 2:
        return "Canceled";
      default:
        return "Unknown status";
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
        case "RegisteredUser": {
          setContent(
            <>
              <RegisteredUserHeader />
              <div className="p-5">
                <div className="flex items-center justify-center mb-5">
                  <select
                    id="cities"
                    className="select select-bordered border-neutral dark:border-white w-full max-w-md text-sm md:text-base"
                    defaultValue={""}
                    onChange={(e) => setSelectedCityId(e.target.value)}
                  >
                    <option hidden value="">
                      Select city
                    </option>
                    {cities?.map((city) => (
                      <option key={city.cityId} value={city.cityId}>
                        {city.name}
                      </option>
                    ))}
                  </select>
                </div>
                {reservations?.length === 0 && (
                  <div className="text-sm md:text-base text-center">
                    No reservations were found in this city
                  </div>
                )}
                {reservations && reservations.length > 0 && (
                  <div className="overflow-x-auto">
                    <table className="table table-xs sm:table-sm md:table-md">
                      <thead>
                        <tr>
                          <th></th>
                          <th>Restaurant name</th>
                          <th>Date</th>
                          <th>Time</th>
                          <th>Party size</th>
                          <th>Status</th>
                          <th></th>
                        </tr>
                      </thead>
                      <tbody>
                        {reservations.map((reservation, index) => {
                          const restaurant = restaurants?.find(
                            (restaurant) =>
                              restaurant.restaurantId ===
                              reservation.restaurantId
                          );

                          if (restaurant) {
                            return (
                              <tr key={reservation.reservationId}>
                                <th>{index + 1}</th>
                                <td>{restaurant.name}</td>
                                <td>{reservation.date}</td>
                                <td>
                                  {reservation.time
                                    .split(":")
                                    .slice(0, 2)
                                    .join(":")}
                                </td>
                                <td>{reservation.partySize}</td>
                                <td>{getStatusName(reservation.status)}</td>
                                <td>
                                  <button
                                    className="btn btn-sm md:btn-md btn-square btn-outline"
                                    onClick={() => {
                                      (
                                        document.getElementById(
                                          `modal-${reservation.reservationId}`
                                        ) as HTMLFormElement
                                      ).showModal();
                                    }}
                                  >
                                    <svg
                                      xmlns="http://www.w3.org/2000/svg"
                                      fill="none"
                                      viewBox="0 0 24 24"
                                      strokeWidth="1.5"
                                      stroke="currentColor"
                                      className="w-5 h-5 md:w-6 md:h-6"
                                    >
                                      <path
                                        strokeLinecap="round"
                                        strokeLinejoin="round"
                                        d="M11.25 11.25l.041-.02a.75.75 0 011.063.852l-.708 2.836a.75.75 0 001.063.853l.041-.021M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-9-3.75h.008v.008H12V8.25z"
                                      />
                                    </svg>
                                  </button>
                                  <dialog
                                    id={`modal-${reservation.reservationId}`}
                                    className="modal"
                                  >
                                    <div className="modal-box">
                                      <h3 className="font-bold text-base md:text-lg">
                                        Detailed information
                                      </h3>
                                      <p className="pt-4 text-xs md:text-sm">
                                        Restaurant information
                                        <br />
                                        Name: {restaurant.name}
                                        <br />
                                        Description: {restaurant.description}
                                        <br />
                                        Street address:{" "}
                                        {restaurant.streetAddress}
                                        <br />
                                        Website URL:{" "}
                                        <a href={restaurant.websiteUrl}>
                                          {restaurant.websiteUrl}
                                        </a>
                                      </p>
                                      <p className="pt-2 text-xs md:text-sm">
                                        Reservation information
                                        <br />
                                        Date: {reservation.date}
                                        <br />
                                        Time: {reservation.time}
                                        <br />
                                        Party size: {reservation.partySize}
                                        <br />
                                        Status:{" "}
                                        {getStatusName(reservation.status)}
                                        <br />
                                        Created at: {reservation.createdAt}
                                      </p>
                                      <div className="modal-action">
                                        <form method="dialog">
                                          <button className="btn">Close</button>
                                        </form>
                                      </div>
                                    </div>
                                  </dialog>
                                </td>
                              </tr>
                            );
                          }
                        })}
                      </tbody>
                    </table>
                  </div>
                )}
              </div>
              <Footer />
            </>
          );
          break;
        }
      }
    })();
  }, [cities, restaurants, reservations]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      const response = await fetch("/api/v1/cities", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.ok) {
        const cities: City[] = await response.json();
        cities.sort((city1, city2) => city1.name.localeCompare(city2.name));
        setCities(cities);
      }
    })();
  }, []);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      if (!selectedCityId) {
        return;
      }

      const response = await fetch(
        `/api/v1/cities/${selectedCityId}/restaurants`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (response.ok) {
        const restaurants: Restaurant[] = await response.json();
        setRestaurants(restaurants);
      }
    })();
  }, [selectedCityId]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      if (!selectedCityId) {
        return;
      }

      const response = await fetch(
        `/api/v1/cities/${selectedCityId}/reservations`,
        {
          method: "GET",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      if (response.ok) {
        const reservations: Reservation[] = await response.json();
        reservations.sort((reservation1, reservation2) => {
          const date1 = new Date(reservation1.createdAt);
          const date2 = new Date(reservation2.createdAt);

          if (date1 > date2) {
            return -1;
          } else if (date1 < date2) {
            return 1;
          } else {
            return 0;
          }
        });
        setReservations(reservations);
      }
    })();
  }, [selectedCityId]);

  return content;
}
