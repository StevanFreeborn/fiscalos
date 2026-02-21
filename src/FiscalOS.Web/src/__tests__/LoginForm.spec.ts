import { describe, it, expect } from 'vitest';

import { flushPromises, mount } from '@vue/test-utils';
import LoginForm from '@/components/LoginForm.vue';

describe('LoginForm', () => {
  it('should have username field', () => {
    const wrapper = mount(LoginForm);

    const username = wrapper.get('#username');

    expect(username.element.tagName).toBe('INPUT');
    expect(username.attributes()['type']).toBe('text');
  });

  it('should have password field', () => {
    const wrapper = mount(LoginForm);

    const password = wrapper.get('#password');

    expect(password.element.tagName).toBe('INPUT');
    expect(password.attributes()['type']).toBe('password');
  });

  it('should require username field on submission', async () => {
    const wrapper = mount(LoginForm, {
      attachTo: document.body,
    });

    await wrapper.get('button').trigger('click');

    expect(wrapper.text()).toContain('Username is required');
  });

  it('should require password field on submission', async () => {
    const wrapper = mount(LoginForm, {
      attachTo: document.body,
    });

    await wrapper.get('button').trigger('click');

    expect(wrapper.text()).toContain('Password is required');
  });

  it('should reset error state when new input is entered', async () => {
    const wrapper = mount(LoginForm, {
      attachTo: document.body,
    });

    await wrapper.get('button').trigger('click');

    expect(wrapper.text()).toContain('Username is required');
    expect(wrapper.text()).toContain('Password is required');

    const usernameInput = wrapper.get('#username');
    const passwordInput = wrapper.get('#password');

    await usernameInput.setValue('Stevan');
    await passwordInput.setValue('password');

    expect(wrapper.text()).not.toContain('Username is required');
    expect(wrapper.text()).not.toContain('Password is required');
  });

  it('should disable login button while submitting', async () => {
    let resolveSubmit;

    const wrapper = mount(LoginForm, {
      attachTo: document.body,
      props: {
        onValidSubmit: () => new Promise(resolve => {
          resolveSubmit = resolve;
        }),
      },
    });

    const usernameInput = wrapper.get('#username');
    const passwordInput = wrapper.get('#password');
    const submitButton = wrapper.get('button');

    await usernameInput.setValue('Stevan');
    await passwordInput.setValue('password');
    await submitButton.trigger('click');

    expect('disabled' in submitButton.attributes()).toBe(true);

    resolveSubmit!();
    await flushPromises();

    expect('disabled' in submitButton.attributes()).toBe(false);
  });

  it('should call the onValidSubmit function when submitted with valid state', async () => {
    let wasCalled = false;

    const wrapper = mount(LoginForm, {
      attachTo: document.body,
      props: {
        onValidSubmit: () => new Promise(resolve => {
          wasCalled = true;
          resolve();
        }),
      },
    });

    const usernameInput = wrapper.get('#username');
    const passwordInput = wrapper.get('#password');
    const submitButton = wrapper.get('button');

    await usernameInput.setValue('Stevan');
    await passwordInput.setValue('password');
    await submitButton.trigger('click');

    expect(wasCalled).toBe(true);
  });
});
