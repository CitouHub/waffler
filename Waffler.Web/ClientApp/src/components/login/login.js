import React from 'react';
import { Button, FormGroup, Input } from "reactstrap";
import { Form, Field } from "react-final-form";

import waffle from '../../assets/images/waffle.png';
import './login.css';

const onSubmit = values => {
    console.log(values);
};

const Login = (props) => {
    return (
        <div className="profile">
            <img src={waffle} alt="Logo" className="waffle" />
            <h1>welcome back!</h1>
            <Form
                onSubmit={onSubmit}
                validate={values => {
                    const errors = {};
                    if (!values.password) {
                        errors.password = "Required";
                    }
                    return errors;
                }}
                render={({ handleSubmit, values, submitting, validating, valid }) => (
                    <form onSubmit={handleSubmit}>
                        <FormGroup>
                            <Field name="password">
                                {({ input, meta }) => (
                                    <div>
                                        <Input
                                            {...input}
                                            type="password"
                                            placeholder="password"
                                            invalid={meta.error && meta.touched}
                                        />
                                        {meta.error && meta.touched && <span>{meta.error}</span>}
                                    </div>
                                )}
                            </Field>
                        </FormGroup>
                        <Button type="submit" color="info" disabled={!valid}>
                            Get waffling!
                        </Button>
                    </form>
                )}
            />
        </div>
    )
};

export default Login;
