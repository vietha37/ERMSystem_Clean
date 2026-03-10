"use client";
import { Table, Button, Modal, Form, Input, DatePicker, Card, Space, Popconfirm, message } from "antd";
import dayjs from "dayjs";
import { useEffect, useState } from "react";
import { getUsers, createUser, updateUser, deleteUser } from "../services/api";

export default function Home() {

  const [users, setUsers] = useState([]);
  const [open, setOpen] = useState(false);
  const [editingUser, setEditingUser] = useState(null);

  const [form] = Form.useForm();

  const loadUsers = async () => {
  try {
    const res = await getUsers();
    setUsers(res?.data || []);
  } catch (error) {
    console.error(error);
    message.error("Failed to load users");
  }
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

    if (user.dateOfBirth) {
      user.dateOfBirth = dayjs(user.dateOfBirth);
    }

    form.setFieldsValue(user);
    setOpen(true);
  };

  const handleDelete = async (id) => {
  try {
    await deleteUser(id);
    message.success("User deleted successfully");
    loadUsers();
  } catch (error) {
    console.error(error);
    message.error("Delete failed");
  }
  };

  const handleSubmit = async () => {
  try {

    const values = await form.validateFields();

    if (values.dateOfBirth) {
      values.dateOfBirth = values.dateOfBirth.format("YYYY-MM-DD");
    }

    // check code trùng
    const codeExists = users.some(
      (u) => u.code === values.code && u.id !== editingUser?.id
    );

    if (codeExists) {
      message.error("Code already exists");
      return;
    }

    // check email trùng
    const emailExists = users.some(
      (u) => u.email === values.email && u.id !== editingUser?.id
    );

    if (emailExists) {
      message.error("Email already exists");
      return;
    }

    if (editingUser) {
      await updateUser(editingUser.id, values);
      message.success("User updated successfully");
    } else {
      await createUser(values);
      message.success("User created successfully");
    }

    setOpen(false);
    loadUsers();
    form.resetFields();

  } catch (error) {
    console.log(error);
  }
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
        <Space>

          <Button type="primary" onClick={() => handleEdit(record)}>
            Edit
          </Button>

          <Popconfirm
            title="Are you sure to delete this user?"
            okText="Yes"
            cancelText="No"
            onConfirm={() => handleDelete(record.id)}
          >
            <Button danger>Delete</Button>
          </Popconfirm>

        </Space>
      )
    }
  ];

  return (
    <div style={{ padding: 40 }}>

      <Card
        title="User Management"
        extra={
          <Button type="primary" onClick={handleAdd}>
            Add User
          </Button>
        }
      >

        <Table
          dataSource={users}
          columns={columns}
          rowKey="id"
          pagination={{ pageSize: 5 }}
        />

      </Card>

      <Modal
        title={editingUser ? "Edit User" : "Add User"}
        open={open}
        onOk={handleSubmit}
        onCancel={() => setOpen(false)}
        okText="Save"
      >

        <Form form={form} layout="vertical">

          {/* CODE */}
          <Form.Item
            name="code"
            label="Code"
            rules={[
              { required: true, message: "Code is required" }
            ]}
          >
            <Input />
          </Form.Item>

          {/* FULL NAME */}
          <Form.Item
            name="fullName"
            label="Full Name"
            rules={[
              { required: true, message: "Full name is required" },
              { min: 6, message: "Full name must be at least 6 characters" }
            ]}
          >
            <Input />
          </Form.Item>

          {/* DATE */}
          <Form.Item name="dateOfBirth" label="Date Of Birth">
            <DatePicker style={{ width: "100%" }} />
          </Form.Item>

          {/* EMAIL */}
          <Form.Item
            name="email"
            label="Email"
            rules={[
              { required: true, message: "Email is required" },
              { type: "email", message: "Email is not valid" }
            ]}
          >
            <Input />
          </Form.Item>

          {/* PHONE */}
          <Form.Item
            name="phone"
            label="Phone"
            rules={[
              { required: true, message: "Phone is required" },
              {
                pattern: /^[0-9]{1,13}$/,
                message: "Phone must be numbers and max 13 digits"
              }
            ]}
          >
            <Input />
          </Form.Item>

          {/* ADDRESS */}
          <Form.Item name="address" label="Address">
            <Input />
          </Form.Item>

        </Form>

      </Modal>

    </div>
  );
}