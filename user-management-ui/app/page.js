"use client";

import dayjs from "dayjs";
import { Table, Button, Modal, Form, Input, DatePicker } from "antd";
import { useEffect, useState } from "react";
import { getUsers, createUser, updateUser, deleteUser } from "../services/api";

export default function Home() {

  const [users, setUsers] = useState([]);
  const [open, setOpen] = useState(false);
  const [editingUser, setEditingUser] = useState(null);

  const [form] = Form.useForm();

  const loadUsers = async () => {
    const res = await getUsers();
    setUsers(res.data);
  };

  useEffect(() => {
    loadUsers();
  }, []);

  const handleAdd = () => {
    setEditingUser(null);
    form.resetFields();
    setOpen(true);
  };

  const handleEdit = (user) => {
    setEditingUser(user);

    // convert date string -> dayjs object cho DatePicker
    if (user.dateOfBirth) {
      user.dateOfBirth = dayjs(user.dateOfBirth);
    }

    form.setFieldsValue(user);
    setOpen(true);
  };

  const handleDelete = async (id) => {
    await deleteUser(id);
    loadUsers();
  };

  const handleSubmit = async () => {

    const values = await form.validateFields();

    // convert DatePicker -> string
    if (values.dateOfBirth) {
      values.dateOfBirth = values.dateOfBirth.format("YYYY-MM-DD");
    }

    if (editingUser) {
      await updateUser(editingUser.id, values);
    } else {
      await createUser(values);
    }

    setOpen(false);
    loadUsers();
  };

  const columns = [
    { title: "Code", dataIndex: "code" },
    { title: "Full Name", dataIndex: "fullName" },
    {
      title: "Date Of Birth",
      dataIndex: "dateOfBirth",
      render: (date) => date ? new Date(date).toLocaleDateString() : ""
    },
    { title: "Email", dataIndex: "email" },
    { title: "Phone", dataIndex: "phone" },
    { title: "Address", dataIndex: "address" },
    {
      title: "Action",
      render: (_, record) => (
        <>
          <Button onClick={() => handleEdit(record)}>Edit</Button>
          <Button danger onClick={() => handleDelete(record.id)}>
            Delete
          </Button>
        </>
      )
    }
  ];

  return (
    <div style={{ padding: 40 }}>

      <h2>User Management</h2>

      <Button type="primary" onClick={handleAdd}>
        Add User
      </Button>

      <Table
        dataSource={users}
        columns={columns}
        rowKey="id"
        style={{ marginTop: 20 }}
      />

      <Modal
        title={editingUser ? "Edit User" : "Add User"}
        open={open}
        onOk={handleSubmit}
        onCancel={() => setOpen(false)}
      >

        <Form form={form} layout="vertical">

          <Form.Item
            name="code"
            label="Code"
            rules={[{ required: true }]}
          >
            <Input />
          </Form.Item>

          <Form.Item name="fullName" label="Full Name">
            <Input />
          </Form.Item>

          <Form.Item name="dateOfBirth" label="Date Of Birth">
            <DatePicker style={{ width: "100%" }} />
          </Form.Item>

          <Form.Item name="email" label="Email">
            <Input />
          </Form.Item>

          <Form.Item name="phone" label="Phone">
            <Input />
          </Form.Item>

          <Form.Item name="address" label="Address">
            <Input />
          </Form.Item>

        </Form>

      </Modal>

    </div>
  );
}