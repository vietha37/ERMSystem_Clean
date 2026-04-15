"use client";

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import toast from "react-hot-toast";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { Modal } from "@/components/ui/Modal";
import { getApiErrorMessage } from "@/services/error";
import { staffUserService } from "@/services/staffUserService";
import { StaffUser, UpdateStaffUserPayload } from "@/services/types";

type FormState = {
  username: string;
  password: string;
  role: "Doctor" | "Receptionist";
};

const initialForm: FormState = {
  username: "",
  password: "",
  role: "Doctor",
};

export default function StaffPage() {
  const [items, setItems] = useState<StaffUser[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const [roleFilter, setRoleFilter] = useState<"" | "Doctor" | "Receptionist">("");
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [mode, setMode] = useState<"create" | "edit">("create");
  const [selected, setSelected] = useState<StaffUser | null>(null);
  const [form, setForm] = useState<FormState>(initialForm);
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearch(search);
      setPageNumber(1);
    }, 400);
    return () => clearTimeout(timer);
  }, [search]);

  const fetchData = useCallback(async () => {
    setIsLoading(true);
    try {
      const data = await staffUserService.getAll(
        pageNumber,
        pageSize,
        roleFilter,
        debouncedSearch
      );
      setItems(data.items);
      setTotalCount(data.totalCount);
      setTotalPages(data.totalPages || 1);
    } catch (error) {
      setItems([]);
      setTotalCount(0);
      setTotalPages(1);
      toast.error(getApiErrorMessage(error, "Failed to load staff list."));
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, pageSize, roleFilter, debouncedSearch]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const openCreateModal = () => {
    setMode("create");
    setSelected(null);
    setForm(initialForm);
    setShowPassword(false);
    setIsModalOpen(true);
  };

  const openEditModal = (user: StaffUser) => {
    setMode("edit");
    setSelected(user);
    setForm({
      username: user.username,
      password: "",
      role: user.role,
    });
    setShowPassword(false);
    setIsModalOpen(true);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setIsSubmitting(true);
    try {
      if (mode === "create") {
        await staffUserService.create({
          username: form.username.trim(),
          password: form.password,
          role: form.role,
        });
        toast.success("Staff account created.");
      } else if (selected) {
        const payload: UpdateStaffUserPayload = {
          username: form.username.trim(),
          role: form.role,
        };
        if (form.password.trim()) {
          payload.password = form.password.trim();
        }
        await staffUserService.update(selected.id, payload);
        toast.success("Staff account updated.");
      }

      setIsModalOpen(false);
      fetchData();
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Failed to save staff account."));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (user: StaffUser) => {
    const ok = window.confirm(`Delete ${user.username} (${user.role})?`);
    if (!ok) {
      return;
    }
    try {
      await staffUserService.delete(user.id);
      toast.success("Staff account deleted.");
      if (items.length === 1 && pageNumber > 1) {
        setPageNumber((p) => p - 1);
      } else {
        fetchData();
      }
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Failed to delete staff account."));
    }
  };

  const titleByMode = useMemo(
    () => (mode === "create" ? "Create Staff Account" : "Edit Staff Account"),
    [mode]
  );

  return (
    <div className="space-y-6 max-w-7xl mx-auto">
      <div className="flex justify-between items-center bg-white p-6 rounded-2xl shadow-sm border border-gray-100">
        <div>
          <h1 className="text-3xl font-bold text-gray-800 tracking-tight">Staff Management</h1>
          <p className="text-gray-500 text-sm mt-1">
            Admin can manage Doctor and Receptionist accounts.
          </p>
        </div>
        <Button onClick={openCreateModal}>+ Add Staff</Button>
      </div>

      <Card className="p-6 border-none shadow-sm rounded-2xl bg-white">
        <div className="mb-6 flex flex-col md:flex-row gap-3 justify-between">
          <input
            type="text"
            placeholder="Search username..."
            className="w-full md:w-[360px] px-4 py-2.5 border border-gray-200 rounded-xl focus:ring-4 focus:ring-blue-500/20 focus:border-blue-500 outline-none transition-all text-sm shadow-sm"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />

          <div className="flex items-center gap-3">
            <select
              className="px-3 py-2 border border-gray-200 rounded-xl text-sm bg-white"
              value={roleFilter}
              onChange={(e) => {
                setRoleFilter(e.target.value as "" | "Doctor" | "Receptionist");
                setPageNumber(1);
              }}
            >
              <option value="">All Roles</option>
              <option value="Doctor">Doctor</option>
              <option value="Receptionist">Receptionist</option>
            </select>

            <select
              className="px-3 py-2 border border-gray-200 rounded-xl text-sm bg-white"
              value={pageSize}
              onChange={(e) => {
                setPageSize(Number(e.target.value));
                setPageNumber(1);
              }}
            >
              <option value={5}>5 / page</option>
              <option value={10}>10 / page</option>
              <option value={20}>20 / page</option>
            </select>
          </div>
        </div>

        <div className="rounded-2xl overflow-hidden border border-gray-100 shadow-sm bg-white">
          <table className="w-full text-left border-collapse">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Username</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider">Role</th>
                <th className="p-4 text-xs font-bold text-gray-500 uppercase tracking-wider text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-50">
              {isLoading ? (
                <tr>
                  <td colSpan={3} className="p-8 text-center text-gray-500">Loading...</td>
                </tr>
              ) : items.length === 0 ? (
                <tr>
                  <td colSpan={3} className="p-8 text-center text-gray-500">
                    No staff found.
                  </td>
                </tr>
              ) : (
                items.map((user) => (
                  <tr key={user.id} className="hover:bg-blue-50/30 transition-colors">
                    <td className="p-4 font-semibold text-gray-800">{user.username}</td>
                    <td className="p-4">
                      <span
                        className={`px-2.5 py-1 rounded-lg text-xs font-bold ${
                          user.role === "Doctor"
                            ? "bg-cyan-50 text-cyan-700 border border-cyan-100"
                            : "bg-amber-50 text-amber-700 border border-amber-100"
                        }`}
                      >
                        {user.role}
                      </span>
                    </td>
                    <td className="p-4 text-right space-x-2">
                      <button
                        onClick={() => openEditModal(user)}
                        className="bg-white border border-gray-200 text-gray-600 hover:text-blue-600 hover:border-blue-300 hover:bg-blue-50 px-3 py-1.5 rounded-lg transition-colors text-sm font-semibold shadow-sm"
                      >
                        Edit
                      </button>
                      <button
                        onClick={() => handleDelete(user)}
                        className="bg-white border border-red-100 text-red-500 hover:bg-red-500 hover:text-white hover:border-red-600 px-3 py-1.5 rounded-lg transition-colors text-sm font-semibold shadow-sm"
                      >
                        Delete
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {!isLoading && totalPages > 0 && (
          <div className="mt-5 flex justify-between items-center text-sm">
            <div className="text-gray-500 font-medium">
              Showing{" "}
              <span className="text-gray-900 font-bold">
                {totalCount === 0 ? 0 : (pageNumber - 1) * pageSize + 1}
              </span>{" "}
              to{" "}
              <span className="text-gray-900 font-bold">
                {Math.min(pageNumber * pageSize, totalCount)}
              </span>{" "}
              of <span className="text-blue-600 font-bold">{totalCount}</span> accounts
            </div>
            <div className="flex gap-1 items-center bg-gray-50 border border-gray-200 p-1 rounded-xl">
              <button
                disabled={pageNumber === 1}
                onClick={() => setPageNumber(1)}
                className="px-3 py-1.5 rounded-lg font-bold disabled:opacity-40 hover:bg-white text-gray-600"
              >
                «
              </button>
              <button
                disabled={pageNumber === 1}
                onClick={() => setPageNumber((p) => p - 1)}
                className="px-3 py-1.5 rounded-lg font-bold disabled:opacity-40 hover:bg-white text-gray-600"
              >
                ‹ Prev
              </button>
              <span className="px-4 py-1.5 font-bold text-blue-700 bg-blue-100/50 rounded-lg">
                {pageNumber} / {totalPages}
              </span>
              <button
                disabled={pageNumber === totalPages}
                onClick={() => setPageNumber((p) => p + 1)}
                className="px-3 py-1.5 rounded-lg font-bold disabled:opacity-40 hover:bg-white text-gray-600"
              >
                Next ›
              </button>
            </div>
          </div>
        )}
      </Card>

      <Modal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title={titleByMode}>
        <form onSubmit={handleSubmit} className="space-y-4 mt-2 px-1 pb-2">
          <div>
            <label className="block text-sm font-bold text-gray-700 mb-1.5">
              Username <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              className="w-full px-4 py-2.5 border border-gray-300 rounded-xl outline-none text-gray-800"
              value={form.username}
              onChange={(e) => setForm((prev) => ({ ...prev, username: e.target.value }))}
              required
            />
          </div>

          <div>
            <label className="block text-sm font-bold text-gray-700 mb-1.5">
              Role <span className="text-red-500">*</span>
            </label>
            <select
              className="w-full px-4 py-2.5 bg-white border border-gray-300 rounded-xl outline-none text-gray-800"
              value={form.role}
              onChange={(e) =>
                setForm((prev) => ({
                  ...prev,
                  role: e.target.value as "Doctor" | "Receptionist",
                }))
              }
              required
            >
              <option value="Doctor">Doctor</option>
              <option value="Receptionist">Receptionist</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-bold text-gray-700 mb-1.5">
              {mode === "create" ? (
                <>
                  Password <span className="text-red-500">*</span>
                </>
              ) : (
                "New Password (optional)"
              )}
            </label>
            <div className="relative">
              <input
                type={showPassword ? "text" : "password"}
                className="w-full px-4 py-2.5 pr-16 border border-gray-300 rounded-xl outline-none text-gray-800"
                value={form.password}
                onChange={(e) => setForm((prev) => ({ ...prev, password: e.target.value }))}
                required={mode === "create"}
                minLength={6}
              />
              <button
                type="button"
                onClick={() => setShowPassword((v) => !v)}
                className="absolute inset-y-0 right-0 px-3 text-xs font-semibold text-gray-500 hover:text-blue-600 transition-colors"
                aria-label={showPassword ? "Hide password" : "Show password"}
                title={showPassword ? "Hide password" : "Show password"}
              >
                {showPassword ? "Hide" : "Show"}
              </button>
            </div>
          </div>

          <div className="pt-6 border-t border-gray-100 flex justify-end gap-3">
            <button
              type="button"
              onClick={() => setIsModalOpen(false)}
              className="px-5 py-2.5 rounded-xl font-bold bg-gray-100 text-gray-600 hover:bg-gray-200"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-6 py-2.5 rounded-xl font-bold bg-blue-600 text-white hover:bg-blue-700 disabled:opacity-50"
            >
              {isSubmitting ? "Saving..." : mode === "create" ? "Create" : "Save Changes"}
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
