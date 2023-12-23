"use client";

import { ReactNode, useEffect, useState } from "react";
import { User } from "../types";
import { useRouter } from "next/navigation";
import { getAccessToken, getDecodedAccessToken } from "../jwt-utils";
import AdminHeader from "../components/admin-header";
import Footer from "../components/footer";

export default function Users() {
  const [content, setContent] = useState<null | ReactNode>(null);
  const [users, setUsers] = useState<null | User[]>(null);
  const router = useRouter();

  const getRoleName = (roleNumber: number) => {
    switch (roleNumber) {
      case 0:
        return "Registered user";
      case 1:
        return "Restaurant manager";
      case 2:
        return "Admin";
      default:
        return "Unknown role";
    }
  };

  const banUser = async (userId: string) => {
    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    await fetch(`/api/v1/users/${userId}/ban`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
    });
  };

  const unbanUser = async (userId: string) => {
    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    await fetch(`/api/v1/users/${userId}/unban`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
    });
  };

  const deleteUser = async (userId: string) => {
    const accessToken = await getAccessToken();
    if (!accessToken) {
      router.push("/login");
      return;
    }

    const response = await fetch(`/api/v1/users/${userId}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${accessToken}`,
      },
    });

    if (response.status === 204) {
      setUsers((prevUsers) => {
        if (prevUsers) {
          return prevUsers.filter((user) => user.userId !== userId);
        }
        return prevUsers;
      });
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
          router.push("/login");
          return;
        case "Admin":
          setContent(
            <>
              <AdminHeader />
              <div className="p-2">
                {users && users.length > 0 && (
                  <div className="overflow-x-auto">
                    <table className="table table-xs sm:table-sm md:table-md">
                      <thead>
                        <tr>
                          <th></th>
                          <th>User id</th>
                          <th>Username</th>
                          <th>Role</th>
                          <th>Is banned</th>
                          <th></th>
                          <th></th>
                        </tr>
                      </thead>
                      <tbody>
                        {users.map((user, index) => (
                          <tr key={user.userId}>
                            <th>{index + 1}</th>
                            <td>{user.userId}</td>
                            <td>{user.username}</td>
                            <td>{getRoleName(user.role)}</td>
                            <td>
                              <div className="form-control">
                                <label
                                  htmlFor={`checkbox-${user.userId}`}
                                  className="label cursor-pointer hidden"
                                >
                                  Is banned
                                </label>
                                <input
                                  id={`checkbox-${user.userId}`}
                                  type="checkbox"
                                  defaultChecked={user.isBanned}
                                  onChange={() => {
                                    if (!user.isBanned) {
                                      banUser(user.userId);
                                    } else {
                                      unbanUser(user.userId);
                                    }
                                  }}
                                  className="checkbox checkbox-sm md:checkbox-md border-black dark:border-base-content"
                                  disabled={user.role === 2}
                                />
                              </div>
                            </td>
                            <td>
                              <button
                                className="btn btn-sm md:btn-md btn-square btn-outline"
                                onClick={() => deleteUser(user.userId)}
                                disabled={user.role === 2}
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
                                    d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0"
                                  />
                                </svg>
                              </button>
                            </td>
                            <td>
                              <button
                                className="btn btn-sm md:btn-md btn-square btn-outline"
                                onClick={() => {
                                  (
                                    document.getElementById(
                                      `modal-${user.userId}`
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
                                id={`modal-${user.userId}`}
                                className="modal"
                              >
                                <div className="modal-box">
                                  <h3 className="font-bold text-base md:text-lg">
                                    Detailed information
                                  </h3>
                                  <p className="pt-4 text-xs md:text-sm">
                                    User id: {user.userId}
                                    <br />
                                    Username: {user.username}
                                    <br />
                                    Email: {user.email}
                                    <br />
                                    First name: {user.firstName}
                                    <br />
                                    Last name: {user.lastName}
                                    <br />
                                    Role: {getRoleName(user.role)}
                                    <br />
                                    Is banned: {`${user.isBanned}`}
                                    <br />
                                    Token validity threshold:{" "}
                                    {user.tokenValidityThreshold}
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
                        ))}
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
    })();
  }, [users]);

  useEffect(() => {
    (async () => {
      const accessToken = await getAccessToken();
      if (!accessToken) {
        router.push("/login");
        return;
      }

      const response = await fetch("/api/v1/users", {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${accessToken}`,
        },
      });

      if (response.ok) {
        const users: User[] = await response.json();
        users.sort((user1, user2) =>
          user1.username.localeCompare(user2.username)
        );
        setUsers(users);
      }
    })();
  }, []);

  return content;
}
